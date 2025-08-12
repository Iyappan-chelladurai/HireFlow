using System;
using System.ComponentModel.DataAnnotations;

namespace HireFlow_MVC.Models
{
    public class JobApplicationViewModel
    {
        public Guid ApplicationId { get; set; }

        [Required(ErrorMessage = "Candidate ID is required.")]
        public Guid CandidateId { get; set; }
        public string? CandidateName { get; set; } // For display in views

        [Required(ErrorMessage = "Job ID is required.")]
        public Guid JobId { get; set; }
        public string? JobTitle { get; set; } // For display in views

        [Required(ErrorMessage = "Resume path is required.")]
        [MaxLength(500)]
        public string? ResumePath { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime AppliedOn { get; set; }

        [MaxLength(50)]
        public string? ApplicationStatus { get; set; } // Shortlisted, Interviewed, etc.

        [MaxLength(1000)]
        public string? InterviewFeedback { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? OfferSentOn { get; set; }

        public bool IsOfferAccepted { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? OnboardedOn { get; set; }
    }
}
