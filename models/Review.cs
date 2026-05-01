using System.ComponentModel.DataAnnotations;

namespace ShreePerfume.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; } // 1 to 5 Stars

        [Required]
        public string Comment { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
    }
}