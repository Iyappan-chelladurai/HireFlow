using Amazon;
using Amazon.Comprehend;
using Amazon.Comprehend.Model;
using Amazon.Runtime;
using Amazon.Textract;
using Amazon.Textract.Model;
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

    // Cache for JD skills
    private static readonly Dictionary<string, List<string>> _jdSkillsCache = new(StringComparer.OrdinalIgnoreCase);

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

    public async Task<CandidateScoreResult> ScoreCandidateAsync(byte[] resumeFile, string fileName, string jobDescription)
    {
        // 1) JD skills
        var jdSkills = await ExtractSkillsFromJD(jobDescription);

        // 2) Resume text
        string resumeText = await ExtractTextFromResume(resumeFile, fileName);

        // 3) Entities
        var entities = await ExtractEntitiesWithComprehend(resumeText);

        // 4) Experience
        int candidateExp = ExtractYears(resumeText);
        var (minExp, maxExp) = ExtractExperienceRange(jobDescription);
        float expScore = CalculateExperienceScore(candidateExp, minExp, maxExp);

        // 5) Skill matching
        var matchedSkills = new List<string>();
        foreach (var skill in jdSkills)
        {
            if (ContainsSkill(resumeText, skill))
                matchedSkills.Add(skill);
        }

        float skillScore = jdSkills.Count == 0 ? 0f : (matchedSkills.Count / (float)jdSkills.Count) * 100f;

        // 6) Final score via ML.NET or fallback
        float finalScore = PredictFinalScore(skillScore, expScore);

        return new CandidateScoreResult
        {
            JdRequiredSkills = jdSkills,
            MatchedSkills = matchedSkills,
            Entities = entities,
            RequiredExperienceMin = minExp,
            RequiredExperienceMax = maxExp,
            CandidateExperience = candidateExp,
            SkillScore = skillScore,
            ExperienceScore = expScore,
            FinalScore = finalScore
        };
    }

    // ===== JD Skills Extraction =====
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

    private List<string> CleanJsonArray(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return new List<string>();
        raw = raw.Replace("```json", "").Replace("```", "").Trim();

        try
        {
            var list = JsonSerializer.Deserialize<List<string>>(raw) ?? new List<string>();
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

    // ===== Resume Text Extraction =====
    private async Task<string> ExtractTextFromResume(byte[] fileBytes, string fileName)
    {
        // Textract
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
        catch { }

        // PdfPig
        try
        {
            using var pdf = PdfDocument.Open(new MemoryStream(fileBytes));
            return string.Join(" ", pdf.GetPages().Select(p => p.Text));
        }
        catch { }

        // DOCX OpenXML
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
        catch { }

        return string.Empty;
    }

    // ===== AWS Comprehend Entities =====
    private async Task<List<string>> ExtractEntitiesWithComprehend(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<string>();

        var request = new DetectEntitiesRequest { LanguageCode = "en", Text = text };
        var response = await _comprehendClient.DetectEntitiesAsync(request);
        return response.Entities.Select(e => e.Text).Distinct().ToList();
    }

    // ===== Candidate Experience Extraction =====
    private int ExtractYears(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;

        text = text.ToLowerInvariant();
        double totalYears = 0.0;

        // 1️⃣ Explicit "X years" or "X+ years"
        var explicitMatches = Regex.Matches(text, @"(\d+(\.\d+)?)\s*(?:\+)?\s*(?:years?|yrs?)");
        foreach (Match m in explicitMatches)
        {
            totalYears += double.Parse(m.Groups[1].Value);
        }

        // 2️⃣ Date ranges (Jan 2012 - Dec 2015, 2018-2021, etc.)
        var dateRangeMatches = Regex.Matches(text,
            @"(?:(?:jan|feb|mar|apr|may|jun|jul|aug|sep|sept|oct|nov|dec)[a-z]*)?\s*(\d{4})\s*[-–to]+\s*(?:(?:jan|feb|mar|apr|may|jun|jul|aug|sep|sept|oct|nov|dec)[a-z]*)?\s*(\d{4}|present)");

        foreach (Match match in dateRangeMatches)
        {
            if (int.TryParse(match.Groups[1].Value, out int startYear))
            {
                int endYear;
                if (match.Groups[2].Value == "present")
                    endYear = DateTime.Now.Year;
                else if (int.TryParse(match.Groups[2].Value, out int parsedEnd))
                    endYear = parsedEnd;
                else
                    continue;

                if (endYear >= startYear)
                    totalYears += (endYear - startYear + 1); // inclusive
            }
        }

        // 3️⃣ Duration in months: "18 months", "6 mos"
        var monthMatches = Regex.Matches(text, @"(\d+(\.\d+)?)\s*(?:months?|mos?)");
        foreach (Match m in monthMatches)
        {
            totalYears += double.Parse(m.Groups[1].Value) / 12.0;
        }

        // 4️⃣ Round fractional years: 1.4 -> 1, 1.6 -> 2
        return (int)Math.Round(totalYears);
    }



    // ===== Extract min/max from JD =====
    private (int min, int max) ExtractExperienceRange(string jdText)
    {
        if (string.IsNullOrWhiteSpace(jdText)) return (0, 0);

        var rangeMatch = Regex.Match(jdText.ToLower(), @"(\d+(\.\d+)?)\s*(?:-|to)\s*(\d+(\.\d+)?)\s*(?:years?|yrs?)");
        if (rangeMatch.Success)
        {
            int minExp = (int)Math.Round(double.Parse(rangeMatch.Groups[1].Value));
            int maxExp = (int)Math.Round(double.Parse(rangeMatch.Groups[3].Value));
            return (minExp, maxExp);
        }

        var plusMatch = Regex.Match(jdText.ToLower(), @"(\d+(\.\d+)?)\s*\+\s*(?:years?|yrs?)");
        if (plusMatch.Success)
        {
            int exp = (int)Math.Round(double.Parse(plusMatch.Groups[1].Value));
            return (exp, exp);
        }

        var singleMatch = Regex.Match(jdText.ToLower(), @"(\d+(\.\d+)?)\s*(?:years?|yrs?)");
        if (singleMatch.Success)
        {
            int exp = (int)Math.Round(double.Parse(singleMatch.Groups[1].Value));
            return (exp, exp);
        }

        return (0, 0);
    }

    // ===== Experience Scoring =====
    private float CalculateExperienceScore(int candidateExp, int minExp, int maxExp)
    {
        if (minExp <= 0) return 100f;

        if (candidateExp <= minExp)
            return (candidateExp / (float)minExp) * 50f;
        else if (candidateExp <= maxExp)
            return 50f + ((candidateExp - minExp) / (float)(maxExp - minExp)) * 50f;
        else
        {
            float extraYears = candidateExp - maxExp;
            float bonus = extraYears * 10f; // extra bonus per year
            return Math.Min(100f + bonus, 150f);
        }
    }

    // ===== Skill Matching =====
    private bool ContainsSkill(string resumeText, string skill)
    {
        if (string.IsNullOrWhiteSpace(resumeText) || string.IsNullOrWhiteSpace(skill)) return false;

        string normResume = NormalizeForMatch(resumeText);
        string normSkill = NormalizeForMatch(skill);

        if (normResume.Contains(normSkill)) return true;

        var compactSkill = Regex.Replace(normSkill, @"\s+", "");
        if (!string.Equals(compactSkill, normSkill) && normResume.Replace(" ", "").Contains(compactSkill))
            return true;

        if (normSkill.EndsWith("s") && normResume.Contains(normSkill.TrimEnd('s')))
            return true;

        return false;
    }

    private string NormalizeForMatch(string text)
    {
        var lower = text.ToLowerInvariant();
        var cleaned = Regex.Replace(lower, @"[^a-z0-9\+\#\.]+", " ");
        return Regex.Replace(cleaned, @"\s+", " ").Trim();
    }

    // ===== Final Score =====
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
}

// ===== ML.NET POCOs =====
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
    public int RequiredExperienceMin { get; set; }
    public int RequiredExperienceMax { get; set; }
    public int CandidateExperience { get; set; }
    public float SkillScore { get; set; }
    public float ExperienceScore { get; set; }
    public float FinalScore { get; set; }
}
