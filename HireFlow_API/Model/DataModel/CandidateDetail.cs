using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireFlow_API.Model.DataModel
{
    public class CandidateDetail
    {
        [Key]
        public Guid CandidateId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual UserAccount User { get; set; }

        [Required]
        public string? ResumePath { get; set; }

        [StringLength(2000)]
        public string? CoverLetter { get; set; }

        [StringLength(100)]
        public string? CurrentJobTitle { get; set; }

        [Range(0, 50)]
        public float? TotalExperienceYears { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ExpectedSalary { get; set; }

        public DateTime? AvailableFrom { get; set; }

        [StringLength(100)]
        public string? PreferredLocation { get; set; }

        public DateTime ProfileCreatedOn { get; set; } = DateTime.UtcNow;

        public virtual ICollection<CandidateDocumentDetail> Documents { get; set; }

        public ICollection<JobApplication> JobApplications { get; set; }

        public ICollection<OfferLetter> OfferLetters { get; set; }
    }

    public class CandidateJobScore
    {
        [Key]
        public int ScoreId { get; set; }

        [Required]
        public Guid JobApplicationId { get; set; }

        [ForeignKey(nameof(JobApplicationId))]
        public virtual JobApplication JobApplication { get; set; }

        // ✅ Core Fit Scoring
        [Range(0, 100)]
        public int ResumeMatchScore { get; set; }   // Resume keywords vs job requirements

        [Range(0, 100)]
        public int SkillsMatchScore { get; set; }   // Skills vs required job skills

        [Range(0, 100)]
        public int ExperienceScore { get; set; }    // Candidate experience vs required

        [Range(0, 100)]
        public int OverallFitScore { get; set; }    // Computed aggregate score

        // ✅ Feedback from Recruiter / System
        [StringLength(2000)]
        public string? Feedback { get; set; }

        [Required]
        public string EvaluatedBy { get; set; }     // Recruiter / System / AI Engine
        public DateTime EvaluatedOn { get; set; } = DateTime.Now;
    }


}
