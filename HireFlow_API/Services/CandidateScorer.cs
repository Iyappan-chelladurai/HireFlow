using Amazon;
using Amazon.Comprehend;
using Amazon.Comprehend.Model;
using Amazon.Runtime;
using Amazon.Textract;
using Amazon.Textract.Model;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using HireFlow_API.Model;
using HireFlow_API.Model.DataModel;
using HireFlow_API.Model.DTOs;
using HireFlow_API.Repositories;
using HireFlow_API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using BlockType = Amazon.Comprehend.BlockType;
using Document = Amazon.Textract.Model.Document;


public interface ICandidateScoringService
{
    Task<CandidateScoreResult> ScoreCandidateAsync(JobApplicationDTO jobApplication);

     Task<string> ScoreCandidatesAsync();
}

public class CandidateScoringService : ICandidateScoringService
{
    private readonly IJobService _jobService;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly ApplicationDbContext _dbContext;
    private readonly IAmazonTextract _textractClient;
    private readonly IAmazonComprehend _comprehendClient;
    private readonly HttpClient _httpClient;
    private readonly string _openAiKey;
    private readonly IConfiguration _config;

    public CandidateScoringService(
        IJobService jobService,
        IJobApplicationRepository jobApplicationRepository,
        ApplicationDbContext dbContext,
        IConfiguration config)
    {
        _jobService = jobService;
        _jobApplicationRepository = jobApplicationRepository;
        _dbContext = dbContext;
        _httpClient = new HttpClient();
        _config = config;
        foreach (var kv in _config.AsEnumerable())
        {
            Console.WriteLine($"{kv.Key} = {kv.Value}");
        }

        // AWS Credentials
        var accessKey =  Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
        var secretKey =  Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
        var region = RegionEndpoint.GetBySystemName(_config["AwsSettings:Region"]);

        var awsCreds = new BasicAWSCredentials(accessKey, secretKey);

        _textractClient = new AmazonTextractClient(awsCreds, region);
        _comprehendClient = new AmazonComprehendClient(awsCreds, region);

        _openAiKey = Environment.GetEnvironmentVariable("OPEN_API_KEY") ?? throw new ArgumentNullException("OpenAI API key missing");
    }


    public async Task<string> ScoreCandidatesAsync()
    {
        var ScroedApps = _dbContext.tbl_CandidatesJobScore.Select(a => a.JobApplicationId).ToList();
        var JobApps = await _dbContext.JobApplications.Include(a => a.Candidate).Include(a => a.Candidate.User).Include(a => a.Job)
                                .Where(a => (a.ApplicationStatus == "Applied"  || a.ApplicationStatus == "On Hold") && !ScroedApps.Contains(a.ApplicationId)).ToListAsync();

        if (JobApps == null)
            return "No  Candidates found...";

         foreach (var item in JobApps)
        {
            JobApplicationDTO jobApp = new JobApplicationDTO();

            jobApp.ApplicationId = item.ApplicationId;
            jobApp.CandidateName = item.Candidate.User.FullName;
            jobApp.CurrentJobTitle = item.Candidate.CurrentJobTitle;
            jobApp.TotalExperienceYears = Convert.ToInt32(item.Candidate.TotalExperienceYears);
            jobApp.NoticePeriodDays = item.Candidate.NoticePeriodDays;
            jobApp.EducationLevel = item.Candidate.EducationLevel;
            jobApp.JobId = item.JobId;
            jobApp.JobDesc = item.Job.JobDescription;
            jobApp.JobTitle = item.Job.JobTitle;
            jobApp.AvailableFrom = item.Candidate.AvailableFrom;
            jobApp.PreferredLocation = item.Candidate.PreferredLocation;
            jobApp.Skills = item.Candidate.Skills;

            await ScoreCandidateAsync(jobApp);
        }
        return "  Candidates Scored...";
    }


    public async Task<CandidateScoreResult> ScoreCandidateAsync(JobApplicationDTO jobApplication)
    {
        var app = await _jobApplicationRepository.GetApplicationByIdAsync(jobApplication.ApplicationId);
      //  var job = await _jobService.RetrieveJobByIdAsync(app.JobId);

        if (string.IsNullOrEmpty(app.ResumePath) || !File.Exists(app.ResumePath))
            throw new FileNotFoundException("Resume file not found.", app.ResumePath);

        // --------------------------
        // 1️⃣ Extract resume text
        // --------------------------
        string resumeText;
        try
        {
            resumeText = await ExtractTextFromResumeUsingTextract(app.ResumePath);
            if (string.IsNullOrWhiteSpace(resumeText))
                throw new Exception("Textract returned empty text.");
        }
        catch
        {
            resumeText = await ExtractTextFromResumeLocally(app.ResumePath);
        }

        // --------------------------
        // 2️⃣ Extract skills
        // --------------------------
        var skills = await ExtractSkillsFromResume(resumeText);

        // --------------------------
        // 3️⃣ Call ChatGPT for scoring
        // --------------------------
        var result = await GetCandidateScoresFromChatGPT(resumeText, jobApplication.JobDesc, jobApplication);

        // --------------------------
        // 4️⃣ Save/update DB
        // --------------------------
        var existingScore = await _dbContext.tbl_CandidatesJobScore
            .FirstOrDefaultAsync(s => s.JobApplicationId == jobApplication.ApplicationId);

        if (existingScore != null)
        {
            existingScore.ResumeMatchScore = result.ResumeMatchScore;
            existingScore.SkillsMatchScore = result.SkillsMatchScore;
            existingScore.ExperienceScore = result.ExperienceScore;
            existingScore.OverallFitScore = result.OverallFitScore;
            existingScore.Feedback = result.Feedback;
            existingScore.EvaluatedBy = result.EvaluatedBy;
            existingScore.EvaluatedOn = result.EvaluatedOn;
        }
        else
        {
            var entity = new CandidateJobScore
            {
                JobApplicationId = result.JobApplicationId,
                ResumeMatchScore = result.ResumeMatchScore,
                SkillsMatchScore = result.SkillsMatchScore,
                ExperienceScore = result.ExperienceScore,
                OverallFitScore = result.OverallFitScore,
                Feedback = result.Feedback,
                EvaluatedBy = result.EvaluatedBy,
                EvaluatedOn = result.EvaluatedOn
            };
            _dbContext.tbl_CandidatesJobScore.Add(entity);
        }

        var updateCandidateCore = _dbContext.JobApplications.Where(a => a.ApplicationId == result.JobApplicationId).FirstOrDefault();

        if (result.OverallFitScore >= 80)
            updateCandidateCore.ApplicationStatus = "Shortlisted";
        else if (result.OverallFitScore >= 50)
            updateCandidateCore.ApplicationStatus = "On Hold";
        else
            updateCandidateCore.ApplicationStatus = "Rejected";


        await _dbContext.SaveChangesAsync();
        return result;
    }

    // --------------------------
    // Resume Extraction
    // --------------------------
    private async Task<string> ExtractTextFromResumeUsingTextract(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        if (ext != ".pdf")
            throw new NotSupportedException("Textract only supports PDF as primary extractor.");

        var bytes = await File.ReadAllBytesAsync(filePath);
        var request = new AnalyzeDocumentRequest
        {
            Document = new Document { Bytes = new MemoryStream(bytes) },
            FeatureTypes = new List<string> { "TABLES", "FORMS" }
        };

        var response = await _textractClient.AnalyzeDocumentAsync(request);

        var textBuilder = new StringBuilder();
        foreach (var block in response.Blocks)
        {
            if (block.BlockType == BlockType.LINE)
                textBuilder.AppendLine(block.Text);
        }
        return textBuilder.ToString();
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

    // --------------------------
    // Skills Extraction (Comprehend)
    // --------------------------
    private async Task<List<string>> ExtractSkillsFromResume(string resumeText)
    {
        var request = new DetectKeyPhrasesRequest
        {
            Text = resumeText,
            LanguageCode = "en"
        };

        var response = await _comprehendClient.DetectKeyPhrasesAsync(request);

        return response.KeyPhrases
                       .Where(p => !string.IsNullOrWhiteSpace(p.Text) && p.Text.Length > 2)
                       .Select(p => p.Text)
                       .Distinct()
                       .ToList();
    }

    // --------------------------
    // ChatGPT Scoring
    // --------------------------
    private async Task<CandidateScoreResult> GetCandidateScoresFromChatGPT(string resumeText, string jobDescription, JobApplicationDTO jobApplication)
    {
        CandidateScoreResult result = new CandidateScoreResult();
        try
        {


            var body = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
        new
        {
            role = "system",
            content = "You are a strict resume reviewer. You must evaluate resumes ONLY based on the job description. Respond ONLY with valid JSON matching this schema: ResumeMatchScore, SkillsMatchScore, ExperienceScore, OverallFitScore, Feedback. No explanations, no extra text, no greetings."
        },
        new
        {
                                role = "user",
                                content = $@"
                    Here is the job description:
                    {jobDescription}

                    Here is the candidate's resume text:
                    {resumeText}

                    Here is the candidate's submitted application model:
                    {JsonConvert.SerializeObject(jobApplication, Formatting.Indented)}

                    Generate the JSON with:
                    - ResumeMatchScore (0-100)
                    - SkillsMatchScore (0-100)
                    - ExperienceScore (0-100)
                    - OverallFitScore (0-100)
                    - Feedback  
                    "
        }
    },
                temperature = 0
            };


            var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            req.Headers.Add("Authorization", $"Bearer {_openAiKey}");
            req.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            var res = await _httpClient.SendAsync(req);
            var resJson = await res.Content.ReadAsStringAsync();




            string json;
            try
            {
                using var doc = JsonDocument.Parse(resJson);
                json = doc.RootElement
                          .GetProperty("choices")[0]
                          .GetProperty("message")
                          .GetProperty("content")
                          .GetString() ?? "{}";
            }
            catch
            {
                json = "{}";
            }

            // ✅ Clean JSON so Newtonsoft won't fail
            json = json.Trim();
            if (json.StartsWith("```"))
            {
                int startIndex = json.IndexOf('{');
                int endIndex = json.LastIndexOf('}');
                if (startIndex >= 0 && endIndex >= 0)
                {
                    json = json.Substring(startIndex, endIndex - startIndex + 1);
                }
            }

            try
            {
                result = JsonConvert.DeserializeObject<CandidateScoreResult>(json);
            }
            catch
            {
                result = new CandidateScoreResult
                {
                    JobApplicationId = jobApplication.ApplicationId,
                    ResumeMatchScore = 50,
                    SkillsMatchScore = 50,
                    ExperienceScore = 50,
                    OverallFitScore = 50,
                    Feedback = "AI evaluation failed. Default scores applied.",
                    EvaluatedBy = "System",
                    EvaluatedOn = DateTime.UtcNow
                };
            }

            result.JobApplicationId = jobApplication.ApplicationId;
            result.EvaluatedBy = "ChatGPT";
            result.EvaluatedOn = DateTime.UtcNow;

        }

        catch (Exception ex)
        { 
        
        
        }


 
        return result;
    }
}

// --------------------------
// Candidate Score Result
// --------------------------
public class CandidateScoreResult
{
    public Guid JobApplicationId { get; set; }
    public int ResumeMatchScore { get; set; }
    public int SkillsMatchScore { get; set; }
    public int ExperienceScore { get; set; }
    public int OverallFitScore { get; set; }
    public string Feedback { get; set; }
    public string EvaluatedBy { get; set; }
    public DateTime EvaluatedOn { get; set; }
}
