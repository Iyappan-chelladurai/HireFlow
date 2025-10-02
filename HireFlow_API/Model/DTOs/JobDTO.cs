using System.ComponentModel.DataAnnotations;

namespace HireFlow_API.Model.DTOs
{
    public class JobDTO
    {
        public Guid JobId { get; set; }
        public string JobTitle { get; set; }

        public string JobSummary { get; set; } = "";
        public string JobDescription { get; set; } = "";
        public string Department { get; set; }
        public string Location { get; set; }
        public decimal? Salary { get; set; }
        public string EmploymentType { get; set; }
        public int Openings { get; set; }
        public string Skills { get; set; }  

        public DateTime PostedOn { get; set; }
        public DateTime? ClosingDate { get; set; }
        public Guid PostedBy { get; set; }
        public int JobStatus { get; set; }
        public int CandidateCount { get; set; }

    }

    // ✅ DTO for Creating a Job (POST)
   

public class CreateJobDTO
    {

        public Guid? JobId { get; set; }

        [Required, MaxLength(150)]
        public string JobTitle { get; set; }

        public string JobSummary { get; set; } = "";
        public string JobDescription { get; set; }

        [MaxLength(100)]
        public string Department { get; set; }

        [MaxLength(100)]
        public string Location { get; set; }

        public decimal? Salary { get; set; }

        [MaxLength(50)]
        public string EmploymentType { get; set; }

        [Range(1, int.MaxValue)]
        public int Openings { get; set; }

        public string Skills { get; set; }

        public DateTime? ClosingDate { get; set; }

        public Guid? PostedBy { get; set; } // matches JobViewModel

        // 🔹 New Fields
        public string Experience { get; set; }
        public string Education { get; set; }

        // 🔹 Optional: If you want to allow draft flag
        public int JobStatus { get; set; } = 0;
    }


    // ✅ DTO for Updating a Job (PUT)
    public class UpdateJobDTO
    {
        public string JobTitle { get; set; }

        public string JobSummary { get; set; } = "";
        public string JobDescription { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public decimal? Salary { get; set; }
        public string EmploymentType { get; set; }
        public int Openings { get; set; }
        public int JobStatus { get; set; } = 0;
        public string Skills { get; set; }
        public DateTime? ClosingDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class JobApplicatiionsForHRDto
    {
        public Guid JobId { get; set; }
        public string JobTitle { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public int Openings { get; set; }

        public string JobSummary { get; set; } = "";

        public string Skills { get; set; }
        public int JobStatus { get; set; } = 0;
        public int CandidateCount { get; set; } // Number of applicants
        public DateTime ClosingDate { get; set; }
    }


}
