using System.ComponentModel.DataAnnotations;

namespace HireFlow_MVC.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public class LoginResponse
    {
        public string Message { get; set; }
        public string JwtToken { get; set; }
    }

}
