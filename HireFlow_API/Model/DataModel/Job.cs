using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireFlow_API.Model.DataModel
{
    public class Job
    {
        [Key]
        public Guid JobId { get; set; }

        [Required, MaxLength(150)]
        public string? JobTitle { get; set; }

        [MaxLength(1000)]
        public string? JobDescription { get; set; }

        [MaxLength(100)]
        public string? Department { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Salary { get; set; }

        [MaxLength(50)]
        public string? EmploymentType { get; set; } // Full-time, Part-time, Internship, etc.

        public int Openings { get; set; } = 0;

        public DateTime PostedOn { get; set; } = DateTime.UtcNow;

        public DateTime? ClosingDate { get; set; }

        [Required]
        public Guid PostedBy { get; set; }

        [ForeignKey("PostedBy")]
        public UserAccount PostedByUser { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<JobApplication> Applications { get; set; }

        public ICollection<OfferLetter> OfferLetters { get; set; }
    }

}
