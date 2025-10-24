using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using HireFlow_API.Model;
using HireFlow_API.Model.DataModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;
using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HireFlow_API.Services
{
    public class DocumentFraudDetectionService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _region;
        private readonly MLContext _mlContext;
        private readonly ITransformer? _mlModel;

        public DocumentFraudDetectionService(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _accessKey = config["AwsSettings:AccessKey"];
            _secretKey = config["AwsSettings:SecretAccessKey"];
            _region = config["AwsSettings:Region"];
            _mlContext = new MLContext();

            if (File.Exists("fraudModel.zip"))
                _mlModel = _mlContext.Model.Load("fraudModel.zip", out _);
        }

        public async Task<CandidateDocumentDetail?> AnalyzeAndUpdateAsync(Guid documentDetailId)
        {
            var document = await _context.CandidateDocumentDetails
                .FirstOrDefaultAsync(d => d.DocumentDetailId == documentDetailId);

            if (document == null || !File.Exists(document.FilePath))
                throw new FileNotFoundException("Document not found.");

            var pdfImages = ConvertPdfToImages(document.FilePath);
            var featuresList = new List<DocumentFeatures>();

            foreach (var imagePath in pdfImages)
                featuresList.Add(await ExtractFeaturesAsync(imagePath));

            var avgFeature = new DocumentFeatures
            {
                TextDensity = featuresList.Average(f => f.TextDensity),
                LabelCount = (int)featuresList.Average(f => f.LabelCount),
                HasScreenshot = featuresList.Any(f => f.HasScreenshot),
                AvgConfidence = featuresList.Average(f => f.AvgConfidence),
                OCRQuality = featuresList.Average(f => f.OCRQuality)
            };

            var fraudScore = PredictFraudScore(avgFeature);
            var isFraud = fraudScore > 0.6m;
            var note = isFraud ? "⚠️ Fraud detected: abnormal layout or image pattern." : "✅ Document appears authentic.";

            document.IsFraudDetected = isFraud;
            document.FraudScore = Math.Round(fraudScore * 100, 2);
            document.VerifiedBy = "AI Fraud Detector";
            document.VerifiedOn = DateTime.UtcNow;
            document.Notes = note;

            await _context.SaveChangesAsync();
            return document;
        }

        private static List<string> ConvertPdfToImages(string pdfPath)
        {
            var images = new List<string>();
            using var doc = PdfDocument.Load(pdfPath);

            for (int i = 0; i < doc.PageCount; i++)
            {
                using var img = doc.Render(i, 300, 300, true);
                string outPath = Path.Combine(Path.GetTempPath(), $"pdf_page_{Guid.NewGuid()}.png");
                img.Save(outPath, ImageFormat.Png);
                images.Add(outPath);
            }

            return images;
        }

        private async Task<DocumentFeatures> ExtractFeaturesAsync(string imagePath)
        {
            var rekognitionClient = new AmazonRekognitionClient(_accessKey, _secretKey, Amazon.RegionEndpoint.GetBySystemName(_region));

            var imageBytes = await File.ReadAllBytesAsync(imagePath);
            using var memStream = new MemoryStream(imageBytes);

            // Detect Labels
            var labelRequest = new DetectLabelsRequest
            {
                Image = new Amazon.Rekognition.Model.Image { Bytes = memStream },
                MaxLabels = 10,
                MinConfidence = 70
            };
            var labelResponse = await rekognitionClient.DetectLabelsAsync(labelRequest);

            bool hasScreenshot = labelResponse.Labels.Any(l => l.Name.Contains("Screenshot", StringComparison.OrdinalIgnoreCase));
            float? avgConfidence = labelResponse.Labels.Count > 0 ? labelResponse.Labels.Average(l => l.Confidence) : 0;

            // Reset stream for next call
            memStream.Position = 0;

            // Detect Text
            var textRequest = new DetectTextRequest
            {
                Image = new Amazon.Rekognition.Model.Image { Bytes = memStream }
            };
            var textResponse = await rekognitionClient.DetectTextAsync(textRequest);

            float textDensity = textResponse.TextDetections.Count / 1000f;
            float? ocrQuality = textResponse.TextDetections.Count > 0 ? textResponse.TextDetections.Average(t => t.Confidence) : 0;

            return new DocumentFeatures
            {
                LabelCount = labelResponse.Labels.Count,
                HasScreenshot = hasScreenshot,
                AvgConfidence = avgConfidence,
                TextDensity = textDensity,
                OCRQuality = ocrQuality
            };
        }

        private decimal PredictFraudScore(DocumentFeatures features)
        {
            if (_mlModel == null)
                return 0.5m;

            var engine = _mlContext.Model.CreatePredictionEngine<DocumentFeatures, FraudPrediction>(_mlModel);
            var prediction = engine.Predict(features);
            return (decimal)prediction.Score;
        }

        public class DocumentFeatures
        {
            public float TextDensity { get; set; }
            public int LabelCount { get; set; }
            public bool HasScreenshot { get; set; }
            public float? AvgConfidence { get; set; }
            public float? OCRQuality { get; set; }
        }

        public class FraudPrediction
        {
            [ColumnName("PredictedLabel")]
            public bool IsAnomaly { get; set; }

            [ColumnName("Score")]
            public float Score { get; set; }
        }
    }
}
