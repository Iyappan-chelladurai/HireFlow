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

        public DateTime ScheduledDate { get; set; }

        [MaxLength(100)]
        public string? InterviewerName { get; set; }

        [MaxLength(500)]
        public string? InterviewMode { get; set; } // e.g., Zoom, In-Person, Phone

        [MaxLength(1000)]
        public string? InterviewFeedback { get; set; }

        [MaxLength(50)]
        public string? InterviewResult { get; set; } // e.g., Passed, Failed, OnHold

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }

}
