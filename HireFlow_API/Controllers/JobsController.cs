using Amazon.Runtime;
using HireFlow_API.Model.DTOs;
using HireFlow_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace HireFlow_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;


        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _openAiKey;

        public JobsController(IJobService jobService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _jobService = jobService;
            _httpClientFactory = httpClientFactory;
            _openAiKey = configuration["OpenAI:ApiKey"];
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobDTO>>> GetAllJobs()
        {
            var jobs = await _jobService.RetrieveAllJobsAsync();
            return Ok(jobs);
        }

        [HttpGet("{jobId}")]
        public async Task<ActionResult<JobDTO>> GetJobById(Guid jobId)
        {
            var job = await _jobService.RetrieveJobByIdAsync(jobId);
            if (job == null) return NotFound();
            return Ok(job);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<JobDTO>> CreateJob(CreateJobDTO newJob)
        {
            var createdJob = await _jobService.CreateNewJobAsync(newJob);
            return CreatedAtAction(nameof(GetJobById), new { jobId = createdJob.JobId }, createdJob);
        }

        [HttpPut("{jobId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateJob(Guid jobId, UpdateJobDTO updatedJob)
        {
            var updated = await _jobService.UpdateJobDetailsAsync(jobId, updatedJob);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{jobId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteJob(Guid jobId)
        {
            var deleted = await _jobService.DeleteJobAsync(jobId);
            if (!deleted) return NotFound();
            return NoContent();
        }

        [HttpPost("generate-jd")]
        public async Task<IActionResult> GenerateJobDescription([FromBody] CreateJobDTO job)
        {
            if (job == null)
                return BadRequest("Job details are required.");

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var (jobDescriptionHtml, skills) = await GenerateJDFromDTO(job, httpClient, _openAiKey);

                return Ok(new
                {
                    jobDescriptionHtml,
                    requiredSkills = skills
                });
            }
            catch (Exception ex)
            {
                // Log exception
                return StatusCode(500, $"Error generating job description: {ex.Message}");
            }
        }

        private async Task<(string JobDescriptionHtml, List<string> Skills)> GenerateJDFromDTO(CreateJobDTO dto, HttpClient _httpClient, string _openAiKey)
        {
            var prompt = $@"
                            You are an AI assistant generating professional job descriptions.
                            Generate a job description in HTML with inline styles. Include sections:
                            Overview (<p>), Responsibilities (<ul><li>...</li></ul>), Requirements (<ul><li>...</li></ul>), Benefits (<ul><li>...</li></ul>).
                            Return a JSON object with:
                            1. 'jobDescriptionHtml': HTML content
                            2. 'requiredSkills': array of strings

                            Job Details:
                            Title: {dto.JobTitle}
                            Department: {dto.Department}
                            Location: {dto.Location}
                            Salary: {(dto.Salary.HasValue ? dto.Salary.Value.ToString("C") : "Not specified")}
                            Employment Type: {dto.EmploymentType}
                            Openings: {dto.Openings}
                            Closing Date: {(dto.ClosingDate.HasValue ? dto.ClosingDate.Value.ToString("yyyy-MM-dd") : "Not specified")}
                            Additional Info: {dto.Skills ?? "N/A"}

                            Return only valid JSON. No explanations or code fences.
                            ";

            var body = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                new { role = "system", content = "You are an AI assistant generating HTML job descriptions with inline styles." },
                new { role = "user", content = prompt }
            },
                temperature = 0
            };

            var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            req.Headers.Add("Authorization", $"Bearer {_openAiKey}");
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var res = await _httpClient.SendAsync(req);
            var resJson = await res.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(resJson);
            string content = doc.RootElement
                                .GetProperty("choices")[0]
                                .GetProperty("message")
                                .GetProperty("content")
                                .GetString() ?? "{}";

            using var responseDoc = JsonDocument.Parse(content);
            var jobDescriptionHtml = responseDoc.RootElement.GetProperty("jobDescriptionHtml").GetString() ?? "";
            var skillsArray = responseDoc.RootElement.GetProperty("requiredSkills").EnumerateArray();
            var skills = skillsArray.Select(s => s.GetString() ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList();

            return (jobDescriptionHtml, skills);
        }
    }
}




