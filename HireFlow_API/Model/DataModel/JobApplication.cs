using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireFlow_API.Model.DataModel
{
    public class JobApplication
    {
        [Key]
        public Guid ApplicationId { get; set; }

        [Required]
        public Guid CandidateId { get; set; }

        [ForeignKey(nameof(CandidateId))]
        public CandidateDetail Candidate { get; set; }

        [Required]
        public Guid JobId { get; set; }

        [ForeignKey(nameof(JobId))]
        public Job Job { get; set; }

        [Required, MaxLength(500)]
        public string? ResumePath { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime AppliedOn { get; set; } = DateTime.Now;

        public string? ApplicationStatus { get; set; } // e.g., "Shortlisted", "Interviewed", "Selected", "Rejected"

        [MaxLength(1000)]
        public string? InterviewFeedback { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? OfferSentOn { get; set; }

        public bool IsOfferAccepted { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? OnboardedOn { get; set; }
    }

}
