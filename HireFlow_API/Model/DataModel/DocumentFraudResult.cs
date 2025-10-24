using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireFlow_API.Model.DataModel
{
    public class DocumentFraudResult
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CandidateDocumnetId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public float FraudScore { get; set; }

        public bool IsSuspicious { get; set; }

        public string DetectedLabels { get; set; } = string.Empty;

        public float AvgConfidence { get; set; }

        public float OCRQuality { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime ScannedOn { get; set; } = DateTime.Now;
    }
}
