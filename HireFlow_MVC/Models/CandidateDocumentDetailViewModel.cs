using System;
using System.ComponentModel.DataAnnotations;

namespace HireFlow_MVC.Models
{
    public class CandidateDocumentDetailViewModel
    {
        public Guid DocumentDetailId { get; set; }

        [Required(ErrorMessage = "Candidate ID is required.")]
        public Guid CandidateId { get; set; }

        public string? CandidateName { get; set; } // optional display property from CandidateDetail

        [Required(ErrorMessage = "Document type is required.")]
        public int DocumentTypeId { get; set; }

        public string? DocumentTypeName { get; set; } // optional display property from DocumentType

        [Required(ErrorMessage = "File name is required.")]
        [MaxLength(255)]
        public string? FileName { get; set; }

        [Required(ErrorMessage = "File path is required.")]
        [MaxLength(500)]
        public string? FilePath { get; set; }

        [MaxLength(10)]
        public string? FileExtension { get; set; }

        [Range(0, 999.99, ErrorMessage = "File size must be valid.")]
        public decimal? FileSizeInMB { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime UploadedOn { get; set; }

        public bool IsVerified { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? VerifiedOn { get; set; }

        [MaxLength(100)]
        public string? VerifiedBy { get; set; }

        public bool IsFraudDetected { get; set; }

        [Range(0, 100, ErrorMessage = "Fraud score must be between 0 and 100.")]
        public decimal? FraudScore { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; }
    }
}
