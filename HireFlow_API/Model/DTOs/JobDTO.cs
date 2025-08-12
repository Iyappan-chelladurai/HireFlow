namespace HireFlow_API.Model.DTOs
{
    public class JobDTO
    {
        public Guid JobId { get; set; }
        public string JobTitle { get; set; }
        public string JobDescription { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public decimal? Salary { get; set; }
        public string EmploymentType { get; set; }
        public int Openings { get; set; }
        public DateTime PostedOn { get; set; }
        public DateTime? ClosingDate { get; set; }
        public Guid PostedBy { get; set; }
        public bool IsActive { get; set; }
    }

    // ✅ DTO for Creating a Job (POST)
    public class CreateJobDTO
    {
        public string JobTitle { get; set; }
        public string JobDescription { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public decimal? Salary { get; set; }
        public string EmploymentType { get; set; }
        public int Openings { get; set; }
        public DateTime? ClosingDate { get; set; }
        public Guid? PostedBy { get; set; }
    }

    // ✅ DTO for Updating a Job (PUT)
    public class UpdateJobDTO
    {
        public string JobTitle { get; set; }
        public string JobDescription { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public decimal? Salary { get; set; }
        public string EmploymentType { get; set; }
        public int Openings { get; set; }
        public DateTime? ClosingDate { get; set; }
        public bool IsActive { get; set; }
    }

}
