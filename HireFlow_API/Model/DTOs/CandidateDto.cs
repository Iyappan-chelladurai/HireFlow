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
}
