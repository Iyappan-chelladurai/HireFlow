using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireFlow_API.Model.DataModel
{
    public class OfferLetter
    {
        [Key]
        public Guid OfferLetterId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CandidateId { get; set; }

        [ForeignKey(nameof(CandidateId))]
        public CandidateDetail Candidate { get; set; }

        [Required]
        public Guid JobId { get; set; }

        [ForeignKey(nameof(JobId))]
        public Job Job { get; set; }

        [Required, MaxLength(255)]
        public string? OfferLetterPath { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? GeneratedOn { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime")]
        public DateTime? SentOn { get; set; }

        public bool IsAccepted { get; set; } = false;

        [Column(TypeName = "datetime")]
        public DateTime? AcceptedOn { get; set; }

        public bool IsActive { get; set; } = true;
    }

}
