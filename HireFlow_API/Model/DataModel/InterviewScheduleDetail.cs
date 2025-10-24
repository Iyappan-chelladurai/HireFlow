using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireFlow_API.Model.DataModel
{
    public class InterviewScheduleDetail
    {
        [Key]
        public Guid InterviewId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ApplicationId { get; set; }

        [ForeignKey(nameof(ApplicationId))]
        public virtual JobApplication JobApplication { get; set; }

        [Required]
        public int RoundNumber { get; set; } = 1; // e.g., 1 = HR, 2 = Technical, 3 = Final

        [Required]
        [Column(TypeName = "datetime")]
        public DateTime ScheduledDate { get; set; }

        [Column(TypeName = "datetime")]
                public DateTime? RescheduledDate { get; set; } // store reschedule info
        [MaxLength(500)]
        public string? RescheduleReason { get; set; }

        [MaxLength(100)]
        public string? InterviewerName { get; set; }

        [MaxLength(100)]
        public string? InterviewMode { get; set; } // e.g., Zoom, In-Person, Phone

        [MaxLength(500)]
        public string? MeetingLink { get; set; } // For virtual interviews

        [MaxLength(1000)]
        public string? InterviewFeedback { get; set; }

        [MaxLength(50)]
        public string? InterviewResult { get; set; } // e.g., Passed, Failed, OnHold

        [MaxLength(50)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Rescheduled, Cancelled

        [Column(TypeName = "datetime")]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedOn { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        public bool IsActive { get; set; } = true;
    }


}
