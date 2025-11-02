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

        [Column(TypeName = "datetime")]
        public DateTime? OfferSentOn { get; set; }

        public bool IsOfferAccepted { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? OnboardedOn { get; set; }

        public string InterviewFeedback { get; set; } = "";

        public string? ApplicationStatus { get; set; } // e.g., "Applied", "Shortlisted", "Interviewed", "Offered", "Hired", "Rejected"

        public ICollection<JobApplicationStatusHistory> StatusHistories { get; set; }
    }



    public class JobApplicationStatusHistory
    {
        [Key]
        public Guid JobApplicationHistoryId { get; set; }

        [Required]
        public Guid ApplicationId { get; set; }

        [ForeignKey(nameof(ApplicationId))]
        public JobApplication JobApplication { get; set; }

        [Required]
        public Guid StatusId { get; set; }

        [ForeignKey(nameof(StatusId))]
        public MST_ApplicationStatus Status { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime ChangedOn { get; set; } = DateTime.Now;

        [MaxLength(1000)]
        public string? Remarks { get; set; } // optional comments or feedback for this status
    }

    public class MST_ApplicationStatus
    {
        [Key]
        public Guid ApplicationStatusId { get; set; }

        [Required, MaxLength(100)]
        public string ApplicationStatusName { get; set; } // e.g., "Applied", "Shortlisted", etc.

        [MaxLength(500)]
        public string? ApplicationDescription { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<JobApplicationStatusHistory> JobApplicationHistories { get; set; }
    }

}
