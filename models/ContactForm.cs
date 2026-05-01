using System.ComponentModel.DataAnnotations;

namespace ShreePerfume.Models
{
    public class ContactForm
    {


        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter your name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject is required")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message cannot be empty")]
        public string Message { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;
        public string? AdminReply { get; set; }
        //public DateTime? RepliedAt { get; set; }


    }
}