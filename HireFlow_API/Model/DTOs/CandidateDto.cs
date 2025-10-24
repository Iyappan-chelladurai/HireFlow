namespace HireFlow_API.Model.DTOs
{
    public class CandidateDto
    {
        public Guid CandidateId { get; set; }
        public Guid UserId { get; set; }

        public string? ResumePath { get; set; }
        public string? CoverLetter { get; set; }
        public string? CurrentJobTitle { get; set; }

        public int? TotalExperienceYears { get; set; }
        public decimal? ExpectedSalary { get; set; }

        public DateTime? AvailableFrom { get; set; }
        public string? PreferredLocation { get; set; }

        public DateTime ProfileCreatedOn { get; set; }
    }

    public class CandidateCardDTO
    {
        public string CandidateName { get; set; }
        public string Position { get; set; }
        public string Status { get; set; } // e.g., "Interview", "Applied"
        public int MatchScore { get; set; } // 0-100
        public string ImageUrl { get; set; }
        public string Experience { get; set; }
        public string Education { get; set; }
        public DateTime AppliedDate { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
    }

}
