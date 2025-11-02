using DocumentFormat.OpenXml.Bibliography;
using HireFlow_API.Model.DataModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireFlow_API.Model.DTOs
{
    public class InterviewScheduleCreateDto
    {
        [Required]
        public Guid ApplicationId { get; set; }

        [Required]
        public int RoundNumber { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        [MaxLength(100)]
        public string? InterviewerName { get; set; }

        [MaxLength(100)]
        public string? InterviewMode { get; set; }

        [MaxLength(500)]
        public string? MeetingLink { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Scheduled";
    }
    public class InterviewScheduleUpdateDto
    {
        [Required]
        public Guid InterviewId { get; set; }

        public DateTime? RescheduledDate { get; set; }

        [MaxLength(500)]
        public string? RescheduleReason { get; set; }

        [MaxLength(100)]
        public string? InterviewerName { get; set; }

        [MaxLength(100)]
        public string? InterviewMode { get; set; }

        [MaxLength(500)]
        public string? MeetingLink { get; set; }

        [MaxLength(1000)]
        public string? InterviewFeedback { get; set; }

        [MaxLength(50)]
        public string? InterviewResult { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
    }

   

public class InterviewScheduleReadDto
    {
        public Guid InterviewId { get; set; }
        public Guid ApplicationId { get; set; }

        public int RoundNumber { get; set; }

        public DateTime ScheduledDate { get; set; }
        public DateTime? RescheduledDate { get; set; }

        public string? RescheduleReason { get; set; }
        public string? InterviewerName { get; set; }
        public string? InterviewMode { get; set; }
        public string? MeetingLink { get; set; }

        public string? InterviewFeedback { get; set; }
        public string? InterviewResult { get; set; }
        public string? Status { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }

        public bool IsActive { get; set; }
    }

 
public class RescheduleDto
    {
        [Required]
        public Guid InterviewId { get; set; }

        [Required]
        public DateTime NewDate { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
    }


    public class CompleteInterviewDto
    {
        public string Candidate { get; set; }
     
        public DateTime? Date { get; set; }

        public string? Result { get; set; }
         
        public string? Position { get; set; }
    }

    public class ScheduleInterviewDto
    {
        [Required]
        public Guid ApplicationId { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        [StringLength(100)]
        public string InterviewerName { get; set; }

        [Required]
        [StringLength(50)]
        public string InterviewMode { get; set; }

        [StringLength(500)]
        public string? MeetingLink { get; set; }

        [Required]
        public int RoundNumber { get; set; } // optional if you want to track rounds

        public string Status { get; set; } = "Scheduled";

        public string interviewLocation { get; set; }

    }



    public class CancelInterviewDto
    {
        [Required]
        public Guid InterviewId { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
    }
}
