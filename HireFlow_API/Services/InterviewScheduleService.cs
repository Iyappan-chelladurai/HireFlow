using DocumentFormat.OpenXml.Packaging;
using HireFlow_API.Controllers;
using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using HireFlow_API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace HireFlow_API.Services
{
    public interface IInterviewScheduleService
    {
        Task<IEnumerable<UpcomingInterviewDto>> GetUpcomingInterviewsAsync();
        Task<IEnumerable<CompleteInterviewDto>> GetCompletedInterviewsAsync();
        Task<InterviewScheduleDetail?> GetInterviewByIdAsync(Guid id);
        Task<InterviewScheduleDetail> ScheduleInterviewAsync(ScheduleInterviewDto dto);
        Task<InterviewScheduleDetail?> RescheduleInterviewAsync(Guid id, DateTime newDate, string reason);
        Task<InterviewScheduleDetail?> UpdateStatusAsync(Guid id, string status, string? feedback = null, string? result = null);

        Task<InterviewQuestionResponse> GenerateInterviewQAAsync(Guid interviewId);

    }

    public class InterviewScheduleService : IInterviewScheduleService
    {
        private readonly IInterviewScheduleRepository _repository;

        private readonly IEmailService _emailService;

        private readonly IJobApplicationService _jobApplicationService;

        private readonly HttpClient _httpClient;

        public InterviewScheduleService(IInterviewScheduleRepository repository, IEmailService emailService,
                                                                IJobApplicationService jobApplicationService , HttpClient httpClient)
        {
            _repository = repository;
            _emailService = emailService;
            _jobApplicationService = jobApplicationService;
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<UpcomingInterviewDto>> GetUpcomingInterviewsAsync()
            => await _repository.GetUpcomingAsync();

        public async Task<IEnumerable<CompleteInterviewDto>> GetCompletedInterviewsAsync()
            => await _repository.GetCompletedAsync();

        public async Task<InterviewScheduleDetail?> GetInterviewByIdAsync(Guid id)
            => await _repository.GetByIdAsync(id);

        public async Task<InterviewScheduleDetail> ScheduleInterviewAsync(ScheduleInterviewDto dto)
        {
            var interview = new InterviewScheduleDetail
            {
                InterviewId = Guid.NewGuid(),
                ApplicationId = dto.ApplicationId,
                ScheduledDate = dto.ScheduledDate,
                InterviewerName = dto.InterviewerName,
                InterviewMode = dto.InterviewMode,
                MeetingLink = dto.MeetingLink,
                RoundNumber = dto.RoundNumber,
                Status = dto.Status,
                CreatedOn = DateTime.Now,
                UpdatedOn = DateTime.Now
            };




            await _repository.AddAsync(interview);
            await _repository.SaveChangesAsync();


            var candidate = await _jobApplicationService.GetCandidatesbyApplicationIdAsync(dto.ApplicationId);

            _emailService.InterviewScheduledEmail(dto, candidate.CandidateName, candidate.JobTitle);

            return interview;
        }

        public async Task<InterviewScheduleDetail?> RescheduleInterviewAsync(Guid id, DateTime newDate, string reason)
        {
            var interview = await _repository.GetByIdAsync(id);
            if (interview == null) return null;

            interview.RescheduledDate = newDate;
            interview.RescheduleReason = reason;
            interview.Status = "Rescheduled";
            interview.UpdatedOn = DateTime.UtcNow;

            await _repository.UpdateAsync(interview);
            await _repository.SaveChangesAsync();
            return interview;
        }

        public async Task<InterviewScheduleDetail?> UpdateStatusAsync(Guid id, string status, string? feedback = null, string? result = null)
        {
            var interview = await _repository.GetByIdAsync(id);
            if (interview == null) return null;

            interview.Status = status;
            interview.InterviewFeedback = feedback ?? interview.InterviewFeedback;
            interview.InterviewResult = result ?? interview.InterviewResult;
            interview.UpdatedOn = DateTime.UtcNow;

            await _repository.UpdateAsync(interview);
            await _repository.SaveChangesAsync();
            return interview;
        }



        public async Task<InterviewQuestionResponse> GenerateInterviewQAAsync(Guid interviewId)
        {
            var resumeText = await ExtractTextFromResumeLocally(await _jobApplicationService.GetCandidateResumeByInterviewId(interviewId));

            if (string.IsNullOrWhiteSpace(resumeText))
                return new InterviewQuestionResponse { Error = "Resume text is required." };

            var apiKey = Environment.GetEnvironmentVariable("OPEN_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
                return new InterviewQuestionResponse { Error = "OpenAI API key is not configured." };

            var systemPrompt = @"
                You are an expert technical interviewer and hiring assistant.
                You generate structured interview questions based on any candidate's resume, regardless of their domain.

                Follow these rules strictly:
                1. Identify the candidate's domain or specialization from their resume.
                2. Create interview questions specific to that role.
                3. Include both domain-related and behavioral questions.
                4. Assign each question:
                   - category (based on topic)
                   - difficulty (Easy, Medium, Hard)
                   - tags (key skills or tools related to the question)
                5. Always return valid JSON array only in this structure:

                [
                  {
                    ""category"": ""<Category Name>"",
                    ""question"": ""<Question Text>"",
                    ""difficulty"": ""<Easy | Medium | Hard>"",
                    ""tags"": [""<Tag1>"", ""<Tag2>"", ""<Tag3>""]
                  }
                ]

                No commentary or text outside the JSON.
            ";

            var userPrompt = $@"
                Analyze the following resume and generate interview questions for a 30-minute interview.
                Include both role-specific and behavioral questions.
                If the resume belongs to a non-technical field (e.g., HR, Finance, Design),
                focus on relevant functional skills instead of programming.

                Resume:
                {resumeText}
            ";

            var payload = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                }
            };

            var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            req.Headers.Add("Authorization", $"Bearer {apiKey}");
            req.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(req);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new InterviewQuestionResponse
                {
                    Error = $"API call failed: {response.StatusCode}",
                    RawResponse = json
                };
            }

            var result = System.Text.Json.JsonDocument.Parse(json);
            var content = result.RootElement
      .GetProperty("choices")[0]
      .GetProperty("message")
      .GetProperty("content")
      .GetString();

            try
            {
                if (string.IsNullOrWhiteSpace(content))
                    throw new JsonException("Empty response from model.");

                // Remove code fences (```json ... ```)
                content = content.Trim();
                if (content.StartsWith("```"))
                {
                    content = content
                        .Replace("```json", "")
                        .Replace("```", "")
                        .Trim();
                }

                // Newtonsoft parsing
                var parsedQuestions = JsonConvert.DeserializeObject<List<InterviewQuestion>>(content);

                if (parsedQuestions == null || parsedQuestions.Count == 0)
                    throw new JsonException("JSON parsed but contains no questions.");

                return new InterviewQuestionResponse
                {
                    Questions = parsedQuestions
                };
            }
            catch (Exception ex)
            {
                return new InterviewQuestionResponse
                {
                    Error = $"JSON parsing failed: {ex.Message}",
                    RawResponse = content
                };
            }
        }
        private async Task<string> ExtractTextFromResumeLocally(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();

            if (ext == ".pdf")
            {
                using var ms = new MemoryStream(await File.ReadAllBytesAsync(filePath));
                using var pdf = PdfDocument.Open(ms);
                var textBuilder = new StringBuilder();
                foreach (var page in pdf.GetPages())
                    textBuilder.AppendLine(page.Text);
                return textBuilder.ToString();
            }
            else if (ext == ".docx")
            {
                using var doc = WordprocessingDocument.Open(filePath, false);
                var body = doc.MainDocumentPart?.Document.Body;
                if (body == null) return string.Empty;
                return string.Join(Environment.NewLine, body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>().Select(t => t.Text));
            }
            else
            {
                return string.Empty;
            }
        }

        public string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return Regex.Replace(input, "<.*?>", string.Empty);
        }

    }
}
