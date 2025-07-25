using System.ComponentModel.DataAnnotations;

namespace HireFlow_API.Model.DataModel
{
    public class UserAccount
    {
        [Key]
        public Guid UserId { get; set; }

        [Required, MaxLength(20)]
        public string UserName { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; }

        [Required, MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [MaxLength(20)]
        public string Role { get; set; }

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [MaxLength(255)]
        public string? ProfileImagePath { get; set; }

        public bool IsAccountActive { get; set; } = true;

        public DateTime AccountCreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginTimestamp { get; set; }
    }


}
