namespace HireFlow_API.Model.DTOs
{
    public class JobApplicationDTO
    {
        public Guid ApplicationId { get; set; }

        public Guid CandidateId { get; set; }

        public Guid JobId { get; set; }

        public string? ResumePath { get; set; }

        public DateTime AppliedOn { get; set; }

        public string? ApplicationStatus { get; set; } // e.g., "Shortlisted", "Interviewed", etc.

        public string? InterviewFeedback { get; set; }

        public DateTime? OfferSentOn { get; set; }

        public bool IsOfferAccepted { get; set; }

        public DateTime? OnboardedOn { get; set; }

        public IFormFile ResumeFile { get; set; } // File in DTO

      
    }



    public class CreateNewApplicationDTO
    {
        public Guid ApplicationId { get; set; }

        public Guid CandidateId { get; set; }

        public Guid JobId { get; set; }

        public string? ResumePath { get; set; }

        public DateTime AppliedOn { get; set; }

        public string? ApplicationStatus { get; set; } // e.g., "Shortlisted", "Interviewed", etc.

        public string? InterviewFeedback { get; set; }

        public DateTime? OfferSentOn { get; set; }

        public bool IsOfferAccepted { get; set; }

        public DateTime? OnboardedOn { get; set; }

        public IFormFile ResumeFile { get; set; } // File in DTO


    }

}
