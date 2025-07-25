using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireFlow_API.Model.DataModel
{
    public class CandidateDocumentDetail
    {
        [Key]
        public Guid DocumentDetailId { get; set; }

        [Required]
        public Guid CandidateId { get; set; }

        [ForeignKey(nameof(CandidateId))]
        public virtual CandidateDetail Candidate { get; set; }

        [Required]
        public int DocumentTypeId { get; set; }

        [ForeignKey(nameof(DocumentTypeId))]
        public virtual DocumentType DocumentType { get; set; }

        [Required, MaxLength(255)]
        public string? FileName { get; set; }

        [Required, MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(10)]
        public string? FileExtension { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? FileSizeInMB { get; set; }

        public DateTime UploadedOn { get; set; } = DateTime.UtcNow;

        public bool IsVerified { get; set; } = false;

        public DateTime? VerifiedOn { get; set; }

        [MaxLength(100)]
        public string? VerifiedBy { get; set; }

        public bool IsFraudDetected { get; set; } = false;

        [Column(TypeName = "decimal(5,2)")]
        public decimal? FraudScore { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }


}
