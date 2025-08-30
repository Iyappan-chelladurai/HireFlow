using Amazon;
using Amazon.Textract;
using Amazon.Textract.Model;
using Amazon.Comprehend;
using Amazon.Comprehend.Model;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

 
    public class CandidateScorerService
    {
        private readonly AmazonTextractClient _textractClient;
        private readonly AmazonComprehendClient _comprehendClient;
        private readonly HttpClient _httpClient;
        private readonly string _openAiKey;

        // ML.NET
        private readonly MLContext _ml;
        private readonly ITransformer? _model;
        private readonly PredictionEngine<CandidateFeatures, CandidatePrediction>? _predEngine;

        // 🔑 Replace with your real keys (don’t hardcode in production!)
        private const string AwsAccessKey = "AKIAXOQ4Y2CL6P4JBGH4";
        private const string AwsSecretKey = "Wphp7HgRVLsATP8pUL8RkduUI7Kl+ikg8vHEkCSf";
        private const string AwsRegion = "ap-south-1";
        private const string OpenAIApiKey = "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";

        public CandidateScorerService()
        {
            _textractClient = new AmazonTextractClient(AwsAccessKey, AwsSecretKey, RegionEndpoint.GetBySystemName(AwsRegion));
            _comprehendClient = new AmazonComprehendClient(AwsAccessKey, AwsSecretKey, RegionEndpoint.GetBySystemName(AwsRegion));
            _httpClient = new HttpClient();
            _openAiKey = OpenAIApiKey;

            string modelPath = null;
            _ml = new MLContext();
            if (!string.IsNullOrWhiteSpace(modelPath) && File.Exists(modelPath))
            {
                _model = _ml.Model.Load(modelPath, out _);
                _predEngine = _ml.Model.CreatePredictionEngine<CandidateFeatures, CandidatePrediction>(_model);
            }
        }


        /// <summary>
        /// Single-call-to-ChatGPT pipeline:
        /// 1) Extract JD skills (ChatGPT once)
        /// 2) Parse resume text (Textract->PdfPig->Docx)
        /// 3) Extract years from JD & resume (regex)
        /// 4) Match JD skills against resume text (normalized)
        /// 5) Score with ML.NET if model available, else rule-based fallback
        /// </summary>
        public async Task<CandidateScoreResult> ScoreCandidateAsync(byte[] resumeFile, string fileName, string jobDescription)
        {
            // 1) JD skills from ChatGPT (ONE CALL)
            var jdSkillsRaw = await ExtractSkillsFromJD(jobDescription);
            var jdSkills = CleanJsonArray(jdSkillsRaw);

            // 2) Resume text
            string resumeText = await ExtractTextFromResume(resumeFile, fileName);

            // 3) Entities (optional, keeps your current flow)
            var entities = await ExtractEntitiesWithComprehend(resumeText);

            // 4) Experience
            int requiredExp = ExtractYears(jobDescription);
            int candidateExp = ExtractYears(resumeText);

            // 5) Skill matching (normalize and tolerate punctuation/spacing)
            var matchedSkills = new List<string>();
            foreach (var skill in jdSkills)
            {
                if (ContainsSkill(resumeText, skill))
                    matchedSkills.Add(skill);
            }

            float skillScore = jdSkills.Count == 0 ? 0f : (matchedSkills.Count / (float)jdSkills.Count) * 100f;
            float expScore = CalculateExperienceScore(candidateExp, requiredExp);

            // 6) Final score via ML.NET (Option 3) or rule-based fallback
            float finalScore = PredictFinalScore(skillScore, expScore);

            return new CandidateScoreResult
            {
                JdRequiredSkills = jdSkills,
                MatchedSkills = matchedSkills,
                Entities = entities,
                RequiredExperience = requiredExp,
                CandidateExperience = candidateExp,
                SkillScore = skillScore,
                ExperienceScore = expScore,
                FinalScore = finalScore
            };
        }

        // ----- ChatGPT: Extract required skills from JD (ONE call) -----
        private async Task<string> ExtractSkillsFromJD(string jobDescription)
        {
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
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "[]";
        }

        // ----- Resume text extraction (Textract -> PdfPig -> Docx) -----
        private async Task<string> ExtractTextFromResume(byte[] fileBytes, string fileName)
        {
            // Try Textract
            try
            {
                var request = new DetectDocumentTextRequest
                {
                    Document = new Document { Bytes = new MemoryStream(fileBytes) }
                };
                var response = await _textractClient.DetectDocumentTextAsync(request);
                var textractText = string.Join(" ", response.Blocks.Where(b => b.BlockType == "LINE").Select(b => b.Text));
                if (!string.IsNullOrWhiteSpace(textractText)) return textractText;
            }
            catch { /* continue */ }

            // Try PdfPig
            try
            {
                using var pdf = PdfDocument.Open(new MemoryStream(fileBytes));
                return string.Join(" ", pdf.GetPages().Select(p => p.Text));
            }
            catch { /* continue */ }

            // Try DOCX via OpenXML
            try
            {
                using var mem = new MemoryStream(fileBytes);
                using var docx = WordprocessingDocument.Open(mem, false);
                var body = docx.MainDocumentPart?.Document?.Body;
                if (body != null)
                {
                    var text = string.Join(" ", body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>().Select(t => t.Text));
                    if (!string.IsNullOrWhiteSpace(text)) return text;
                }
            }
            catch { /* continue */ }

            return string.Empty;
        }

        // ----- AWS Comprehend Entities (optional) -----
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

        // ----- Years extraction (regex) -----
        private int ExtractYears(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;

            text = text.ToLowerInvariant();

            // 5-7 years, 3 to 5 yrs
            var range = Regex.Match(text, @"(\d+)\s*(?:-|to)\s*(\d+)\s*(?:years?|yrs?)");
            if (range.Success)
            {
                int a = int.Parse(range.Groups[1].Value);
                int b = int.Parse(range.Groups[2].Value);
                return (a + b) / 2;
            }

            // 3+ years
            var plus = Regex.Match(text, @"(\d+)\s*\+\s*(?:years?|yrs?)");
            if (plus.Success) return int.Parse(plus.Groups[1].Value);

            // 3 years
            var single = Regex.Match(text, @"(\d+)\s*(?:years?|yrs?)");
            if (single.Success) return int.Parse(single.Groups[1].Value);

            return 0;
        }

        // ----- Clean JSON array (remove leftover fences if any) -----
        private List<string> CleanJsonArray(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return new List<string>();
            raw = raw.Replace("```json", "").Replace("```", "").Trim();

            try
            {
                var list = JsonSerializer.Deserialize<List<string>>(raw) ?? new List<string>();
                // Normalize values: trim and dedup
                return list
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        // ----- Fuzzy-ish skill contains (normalize punctuation/spacing/case) -----
        private bool ContainsSkill(string resumeText, string skill)
        {
            if (string.IsNullOrWhiteSpace(resumeText) || string.IsNullOrWhiteSpace(skill)) return false;

            string normResume = NormalizeForMatch(resumeText);
            string normSkill = NormalizeForMatch(skill);

            // direct contains
            if (normResume.Contains(normSkill)) return true;

            // Try simple variants: remove dots/spaces from skill
            var compactSkill = Regex.Replace(normSkill, @"\s+", "");
            if (!string.Equals(compactSkill, normSkill) && normResume.Replace(" ", "").Contains(compactSkill))
                return true;

            // Basic plural/singular tweak (very light)
            if (normSkill.EndsWith("s") && normResume.Contains(normSkill.TrimEnd('s')))
                return true;

            return false;
        }

        private string NormalizeForMatch(string text)
        {
            // lower, keep letters/digits/#/+/., convert other to space, then squeeze spaces
            var lower = text.ToLowerInvariant();
            var cleaned = Regex.Replace(lower, @"[^a-z0-9\+\#\.]+", " ");
            return Regex.Replace(cleaned, @"\s+", " ").Trim();
        }

        // ----- Experience score (0..100) -----
        private float CalculateExperienceScore(int candidateExp, int requiredExp)
        {
            if (requiredExp <= 0) return 100f;
            if (candidateExp >= requiredExp) return 100f;
            return (candidateExp / (float)requiredExp) * 100f;
        }

        // ----- Option 3: ML.NET Final Score (fallback to rule-based if model missing) -----
        private float PredictFinalScore(float skillScore, float expScore)
        {
            if (_predEngine != null)
            {
                var pred = _predEngine.Predict(new CandidateFeatures
                {
                    SkillScore = skillScore,
                    ExperienceScore = expScore
                });
                // Clamp to 0..100 just in case the model outputs outside bounds
                return Math.Clamp(pred.Score, 0f, 100f);
            }

            // Fallback rule-based weights
            return (skillScore * 0.6f) + (expScore * 0.4f);
        }
    }

    // ===== ML.NET feature/prediction POCOs =====
    public class CandidateFeatures
    {
        [LoadColumn(0)] public float SkillScore { get; set; }
        [LoadColumn(1)] public float ExperienceScore { get; set; }
    }

    public class CandidatePrediction
    {
        [ColumnName("Score")] public float Score { get; set; }
    }

    // ===== Result DTO =====
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
    }
 
