using System.ComponentModel.DataAnnotations;

namespace ShreePerfume.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; } // Kis Order se juda hai
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; } // Us waqt perfume ki kya price thi
    }
}