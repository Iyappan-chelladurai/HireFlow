using Amazon;
using Amazon.Comprehend;
using Amazon.Comprehend.Model;
using Amazon.Textract;
using Amazon.Textract.Model;
using DocumentFormat.OpenXml.Packaging;
using HireFlow_API.Services;
using Humanizer;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace HireFlow.Services
{
    public class CandidateScorerService
    {
        private readonly AmazonTextractClient _textractClient;
        private readonly AmazonComprehendClient _comprehendClient;
        private readonly HttpClient _httpClient;
        private readonly string _openAiKey;
        private readonly IConfiguration _configuration;

        private readonly IJobApplicationService _jobApplicationService;
        private readonly IJobService _jobService;



        // ML.NET
        private readonly MLContext _ml;
        private readonly ITransformer? _model;
        private readonly PredictionEngine<CandidateFeatures, CandidatePrediction>? _predEngine;


        // 🔑 Replace with your real keys (don’t hardcode in production!)

        private const string awsAccessKey = "AKIAXOQ4Y2CL6P4JBGH4";
        private const string awsSecretKey = "Wphp7HgRVLsATP8pUL8RkduUI7Kl+ikg8vHEkCSf";
        private const string awsRegion = "ap-south-1";
        private const string OpenAIApiKey = "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";

        // Cache for JD skills
        private static readonly Dictionary<string, List<string>> _jdSkillsCache = new(StringComparer.OrdinalIgnoreCase);
        public CandidateScorerService(IConfiguration configuration, IJobApplicationService JobApplicationService, 
                                      IJobService jobService, string? modelPath = null)
        {
            _textractClient = new AmazonTextractClient(awsAccessKey, awsSecretKey, RegionEndpoint.GetBySystemName(awsRegion));
            _comprehendClient = new AmazonComprehendClient(awsAccessKey, awsSecretKey, RegionEndpoint.GetBySystemName(awsRegion));
            _httpClient = new HttpClient();
            _openAiKey = OpenAIApiKey;

            _ml = new MLContext();
            if (!string.IsNullOrWhiteSpace(modelPath) && File.Exists(modelPath))
            {
                _model = _ml.Model.Load(modelPath, out _);
                _predEngine = _ml.Model.CreatePredictionEngine<CandidateFeatures, CandidatePrediction>(_model);
            }

            _configuration = configuration;

              _jobApplicationService = JobApplicationService;
                _jobService = jobService;
        }

        // =======================
        // Main Scoring Function
        // =======================
        public async Task<CandidateScoreResult> ScoreCandidateAsync(Guid JobId, Guid JobApplicationId)
        {
            var app = await _jobApplicationService.RetrieveApplicationDetailsAsync(JobApplicationId);
            var job = await _jobService.RetrieveJobByIdAsync(JobId);

            if (string.IsNullOrEmpty(app.ResumePath) || !System.IO.File.Exists(app.ResumePath))
            {
                throw new FileNotFoundException("Resume file not found.", app.ResumePath);
            }

            var resumeFile = await System.IO.File.ReadAllBytesAsync(app.ResumePath);
                var fileName = Path.GetFileName(app.ResumePath);
            string resumeText = await ExtractTextFromResume(resumeFile, fileName);

                var jdSkills = await ExtractSkillsFromJD(job.JobDescription);

                var entities = await ExtractEntitiesWithComprehend(resumeText);
                int candidateExpFromDates = ExtractYearsFromResumeDates(resumeFile, fileName);
                int candidateExperience = ExtractCandidateExperience(resumeText, candidateExpFromDates);
                int requiredMinExp = ExtractExperienceMin(job.JobDescription);
                int requiredMaxExp = ExtractExperienceMax(job.JobDescription);

                var matchedSkills = new List<string>();

                foreach (var skill in jdSkills)
                {
                    if (ContainsSkill(resumeText, skill))
                        matchedSkills.Add(skill);
                }
                float skillScore = jdSkills.Count == 0 ? 0f : (matchedSkills.Count / (float)jdSkills.Count) * 100f;

                float expScore = CalculateExperienceScore(requiredMinExp, requiredMaxExp, candidateExperience);

                float finalScore = PredictFinalScore(skillScore, expScore);

                return new CandidateScoreResult
                {
                    JdRequiredSkills = jdSkills,
                    MatchedSkills = matchedSkills,
                    Entities = entities,
                    RequiredExperience = requiredMaxExp,
                    CandidateExperience = candidateExperience,
                    SkillScore = skillScore,
                    ExperienceScore = expScore,
                    FinalScore = finalScore
                };
        }

        // -------------------------------
        // Candidate Experience Extraction
        // -------------------------------
        private int ExtractCandidateExperience(string resumeText, int candidateExpFromDates)
        {
            if (candidateExpFromDates > 0)
                return candidateExpFromDates;

            return ExtractYears(resumeText);
        }

        // -------------------------------
        // Normalize Experience Score
        // -------------------------------
        private float CalculateExperienceScore(int requiredMin, int requiredMax, int candidateExp)
        {
            if (requiredMax > requiredMin)
            {
                if (candidateExp < requiredMin)
                    return (candidateExp * 100f) / requiredMin;
                if (candidateExp >= requiredMin && candidateExp <= requiredMax)
                    return 100f;
                return 100f; // overqualified
            }
            else
            {
                if (candidateExp == requiredMin)
                    return 100f;
                if (candidateExp < requiredMin)
                    return (candidateExp * 100f) / requiredMin;
                return 100f;
            }
        }

        // -------------------------------
        // Predict Final Score (ML or fallback)
        // -------------------------------
        private float PredictFinalScore(float skillScore, float expScore)
        {
            if (_predEngine != null)
            {
                var pred = _predEngine.Predict(new CandidateFeatures
                {
                    SkillScore = skillScore,
                    ExperienceScore = expScore
                });
                return Math.Clamp(pred.Score, 0f, 100f);
            }
            return (skillScore * 0.6f) + (expScore * 0.4f);
        }

        // =======================
        // Skill Extraction (ChatGPT)
        // =======================
        // ===== JD Skills Extraction =====
        //
        private async Task<List<string>> ExtractSkillsFromJD(string jobDescription)
        {
            if (_jdSkillsCache.TryGetValue(jobDescription, out var cachedSkills))
                return cachedSkills;

            var body = new
            {
                model = "gpt-4o-mini",
                messages = new[]
            {
                new { role = "system", content = "Extract the key required skills from the following job description. Return ONLY a valid JSON array of strings (e.g., [\"C#\",\"SQL\"]). No code fences, no explanations." },
                new { role = "user", content = jobDescription }
            },
                temperature = 0
            };
            var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            req.Headers.Add("Authorization", $"Bearer {_openAiKey}");
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var res = await _httpClient.SendAsync(req); 
            var resJson = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resJson);
            string rawSkills = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "[]";
            var cleanedSkills = CleanJsonArray(rawSkills);
            _jdSkillsCache[jobDescription] = cleanedSkills;
            return cleanedSkills;
        }

        // =======================
        // Resume Text Extraction
        // =======================
        private async Task<string> ExtractTextFromResume(byte[] fileBytes, string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();

            try
            {
                switch (ext)
                {
                    case ".pdf":
                        try
                        {
                            using var pdf = PdfDocument.Open(new MemoryStream(fileBytes));
                            var text = string.Join(" ", pdf.GetPages().Select(p => p.Text));
                            if (!string.IsNullOrWhiteSpace(text)) return text;
                        }
                        catch (Exception ex)
                        {
                            return ex.InnerException?.Message ?? ex.Message;
                        }
                        break;

                    case ".docx":
                        try
                        {
                            using var mem = new MemoryStream(fileBytes);
                            using var docx = WordprocessingDocument.Open(mem, false);
                            var body = docx.MainDocumentPart?.Document?.Body;
                            if (body != null)
                            {
                                var text = string.Join(" ", body
                                    .Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>()
                                    .Select(t => t.Text));
                                if (!string.IsNullOrWhiteSpace(text)) return text;
                            }
                        }
                        catch (Exception ex)
                        {
                            return ex.InnerException?.Message ?? ex.Message;
                        }
                        break;

                    case ".jpg":
                    case ".jpeg":
                    case ".png":
                    case ".tiff":
                        try
                        {
                            var request = new DetectDocumentTextRequest
                            {
                                Document = new Document { Bytes = new MemoryStream(fileBytes) }
                            };
                            var response = await _textractClient.DetectDocumentTextAsync(request);
                            var textractText = string.Join(" ",
                                response.Blocks.Where(b => b.BlockType == "LINE").Select(b => b.Text));
                            if (!string.IsNullOrWhiteSpace(textractText)) return textractText;
                        }
                        catch (Exception ex)
                        {
                            return ex.InnerException?.Message ?? ex.Message;
                        }
                        break;

                    default:
                        // Fallback attempt if extension is unknown
                        try
                        {
                            var request = new DetectDocumentTextRequest
                            {
                                Document = new Document { Bytes = new MemoryStream(fileBytes) }
                            };
                            var response = await _textractClient.DetectDocumentTextAsync(request);
                            var textractText = string.Join(" ",
                                response.Blocks.Where(b => b.BlockType == "LINE").Select(b => b.Text));
                            if (!string.IsNullOrWhiteSpace(textractText)) return textractText;
                        }
                        catch (Exception ex)
                        {
                            return ex.InnerException?.Message ?? ex.Message;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                return ex.InnerException?.Message ?? ex.Message;
            }

            return string.Empty;
        }


        // =======================
        // AWS Comprehend Entities
        // =======================
        private async Task<List<string>> ExtractEntitiesWithComprehend(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return new List<string>();

            var request = new DetectEntitiesRequest
            {
                LanguageCode = "en",
                Text = text
            };

            var response = await _comprehendClient.DetectEntitiesAsync(request);
            return response.Entities.Select(e => e.Text).Distinct().ToList();
        }

        // =======================
        // Skill Matching Helpers
        // =======================
        private List<string> CleanJsonArray(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return new List<string>();
            raw = raw.Replace("```json", "").Replace("```", "").Trim();
            try
            {
                var list = JsonSerializer.Deserialize<List<string>>(raw) ?? new List<string>();
                return list.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        private bool ContainsSkill(string resumeText, string skill)
        {
            if (string.IsNullOrWhiteSpace(resumeText) || string.IsNullOrWhiteSpace(skill)) 
                return false;

            string normResume = Regex.Replace(resumeText.ToLowerInvariant(), @"[^a-z0-9\+\#\.]+", " ");
            string normSkill = Regex.Replace(skill.ToLowerInvariant(), @"[^a-z0-9\+\#\.]+", " ");
            if (normResume.Contains(normSkill)) 
                return true;

            var compactSkill = Regex.Replace(normSkill, @"\s+", "");
            if (!string.Equals(compactSkill, normSkill) && normResume.Replace(" ", "").Contains(compactSkill))
                return true;

            if (normSkill.EndsWith("s") && normResume.Contains(normSkill.TrimEnd('s'))) return true;
            return false;
        }

        // =======================
        // JD Experience Extractors
        // =======================
        private int ExtractExperienceMin(string jdText)
        {
            var range = Regex.Match(jdText, @"(\d+)\s*(?:-|to)\s*(\d+)\s*(?:years?|yrs?)");
            if (range.Success)
                return int.Parse(range.Groups[1].Value);

            var plus = Regex.Match(jdText, @"(\d+)\s*\+\s*(?:years?|yrs?)");
            if (plus.Success)
                return int.Parse(plus.Groups[1].Value);

            var single = Regex.Match(jdText, @"(\d+)\s*(?:years?|yrs?)");
            if (single.Success)
                return int.Parse(single.Groups[1].Value);

            return 0;
        }

        private int ExtractExperienceMax(string jdText)
        {
            var range = Regex.Match(jdText, @"(\d+)\s*(?:-|to)\s*(\d+)\s*(?:years?|yrs?)");
            if (range.Success)
                return int.Parse(range.Groups[2].Value);

            var plus = Regex.Match(jdText, @"(\d+)\s*\+\s*(?:years?|yrs?)");
            if (plus.Success)
                return int.Parse(plus.Groups[1].Value);

            var single = Regex.Match(jdText, @"(\d+)\s*(?:years?|yrs?)");
            if (single.Success)
                return int.Parse(single.Groups[1].Value);

            return 0;
        }

        // =======================
        // Placeholder for Date-based Experience Extraction
        // =======================
        private int ExtractYearsFromResumeDates(byte[] resumeFile, string fileName)
        {
            // TODO: Implement date parsing from work history
            // For now, return 0 to fallback to summary text
            return 0;
        }

        // =======================
        // Years Extraction from Text
        // =======================
        private int ExtractYears(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            text = text.ToLowerInvariant();
            var range = Regex.Match(text, @"(\d+)\s*(?:-|to)\s*(\d+)\s*(?:years?|yrs?)");
            if (range.Success)
                return (int.Parse(range.Groups[1].Value) + int.Parse(range.Groups[2].Value)) / 2;

            var plus = Regex.Match(text, @"(\d+)\s*\+\s*(?:years?|yrs?)");
            if (plus.Success)
                return int.Parse(plus.Groups[1].Value);

            var single = Regex.Match(text, @"(\d+)\s*(?:years?|yrs?)");
            if (single.Success)
                return int.Parse(single.Groups[1].Value);

            return 0;
        }
    }

    // =======================
    // ML.NET POCOs
    // =======================
    public class CandidateFeatures
    {
        [LoadColumn(0)] public float SkillScore { get; set; }
        [LoadColumn(1)] public float ExperienceScore { get; set; }
    }

    public class CandidatePrediction
    {
        [ColumnName("Score")] public float Score { get; set; }
    }

    // =======================
    // DTO for result
    // =======================
    public class CandidateScoreResult
    {
        public List<string> JdRequiredSkills { get; set; } = new();
        public List<string> MatchedSkills { get; set; } = new();
        public List<string> Entities { get; set; } = new();
        public int RequiredExperience { get; set; }
        public int CandidateExperience { get; set; }
        public float SkillScore { get; set; }
        public float ExperienceScore { get; set; }
        public float FinalScore { get; set; }

        public string? CandidateFeedBack { get; set; }
    }
}
