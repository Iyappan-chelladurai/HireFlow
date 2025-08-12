using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HireFlow_MVC.Models
{
    public class CandidateDetailViewModel
    {
        public Guid CandidateId { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        public Guid UserId { get; set; }

        public string? UserName { get; set; } // from UserAccount if you want to display

        [Required(ErrorMessage = "Resume path is required.")]
        public string? ResumePath { get; set; }

        [StringLength(2000)]
        public string? CoverLetter { get; set; }

        [StringLength(100)]
        public string? CurrentJobTitle { get; set; }

        [Range(0, 50, ErrorMessage = "Experience must be between 0 and 50 years.")]
        public float? TotalExperienceYears { get; set; }

        [Range(0, 999999999.99, ErrorMessage = "Invalid salary.")]
        public decimal? ExpectedSalary { get; set; }

        [DataType(DataType.Date)]
        public DateTime? AvailableFrom { get; set; }

        [StringLength(100)]
        public string? PreferredLocation { get; set; }

        public DateTime ProfileCreatedOn { get; set; }

        // Related details — usually other view models or summaries
        public List<CandidateDocumentDetailViewModel> Documents { get; set; } = new();
        public List<JobApplicationViewModel> JobApplications { get; set; } = new();
        public List<OfferLetterViewModel> OfferLetters { get; set; } = new();
    }

 
 
    public class OfferLetterViewModel
    {
        public Guid OfferId { get; set; }
        public string? JobTitle { get; set; }
        public DateTime IssuedOn { get; set; }
    }
}
