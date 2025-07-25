using Microsoft.AspNetCore.Identity;

namespace HireFlow_MVC.Models.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
     
        public string ProfileImagePath { get; set; }

        public bool IsAccountActive { get; set; } = true;

        public DateTime AccountCreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginTimestamp { get; set; }
    }
}
