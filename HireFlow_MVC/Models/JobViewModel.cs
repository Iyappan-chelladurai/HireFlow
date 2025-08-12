using System.ComponentModel.DataAnnotations;

namespace HireFlow_MVC.Models
{
    public class JobViewModel
    {
        public Guid? JobId { get; set; }  // Inc

        [Required, MaxLength(150)]
        public string JobTitle { get; set; }

        [MaxLength(1000)]
        public string JobDescription { get; set; }

        [MaxLength(100)]
        public string Department { get; set; }

        [MaxLength(100)]
        public string Location { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Salary { get; set; }

        [MaxLength(50)]
        public string EmploymentType { get; set; }

        [Range(1, int.MaxValue)]
        public int Openings { get; set; }

        public DateTime? ClosingDate { get; set; }
    }

}
