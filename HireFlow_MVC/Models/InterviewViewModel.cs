namespace HireFlow_MVC.Models
{
    public class InterviewDetailsDTO
    {
        public Guid? InterviewId { get; set; }

        public Guid ApplicationId { get; set; }

        public DateTime ScheduledDate { get; set; }

        public string? InterviewerName { get; set; }

        public string? InterviewMode { get; set; } // e.g., Zoom, In-Person, Phone

        public string? InterviewFeedback { get; set; }

        public string? InterviewResult { get; set; } // e.g., Passed, Failed, OnHold

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
