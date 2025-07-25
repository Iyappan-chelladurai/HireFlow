using System.ComponentModel.DataAnnotations;

namespace HireFlow_MVC.Models.Data
{
    public class CreateUserViewModel
    {
      
        public string UserName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
