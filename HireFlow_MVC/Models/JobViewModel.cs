using System.ComponentModel.DataAnnotations;

namespace HireFlow_MVC.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class JobViewModel
    {
        public Guid? JobId { get; set; }  // For updates / internal use

        [Required, MaxLength(150)]
        public string JobTitle { get; set; }

        public string JobSummary { get; set; }
        public string JobDescription { get; set; }

        [MaxLength(100)]
        public string Department { get; set; }

        [MaxLength(100)]
        public string Location { get; set; }

        public string Skills { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Salary { get; set; }

        [MaxLength(50)]
        public string EmploymentType { get; set; }

        [Range(1, int.MaxValue)]
        public int Openings { get; set; }

        public DateTime? ClosingDate { get; set; }

        // 🔹 Experience & Education (from DTO)
        public string Experience { get; set; }
        public string Education { get; set; }

        // 🔹 PostedBy as Guid
        public Guid? PostedBy { get; set; }

        // 🔹 Active flag
        public int JobStatus { get; set; }

        // 🔹 Internal / UI only
        public JobApplicationViewModel? NewApplicationViewData { get; set; } = new JobApplicationViewModel();

        public int? CandidateCount { get; set; }
    }


}
