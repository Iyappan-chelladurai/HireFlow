using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireFlow_API.Model.DataModel
{
    public class DocumentType
    {
        [Key]
        public int DocumentTypeId { get; set; }

        [Required, MaxLength(100)]
        public string? DocumentName { get; set; }

        [Required, MaxLength(50)]
        public string? Code { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        public bool IsMandatory { get; set; } = false;

        [MaxLength(255)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? FileFormat { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? MaxFileSizeMB { get; set; }

        public bool IsVerificationRequired { get; set; } = false;

        public int FraudCheckLevel { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        public virtual ICollection<CandidateDocumentDetail> CandidateDocuments { get; set; }
    }


}
