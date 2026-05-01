using System.ComponentModel.DataAnnotations;

namespace ShreePerfume.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; } // Automatic ID generate hogi

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string? Address { get; set; }

        public string? PhoneNo { get; set; }

        public string? Gender { get; set; }

        public string? ProfilePhotoPath { get; set; } // Sirf image ka naam save hoga

        public string Role { get; set; } = "Customer"; // Default role customer rahega

        public bool IsEmailVerified { get; set; } = false;
        public string? VerificationCode { get; set; }
    }
}