using System.ComponentModel.DataAnnotations;

namespace ShreePerfume.Models
{
    public class Order 
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; } // Login kiye huye user ki ID
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Shipped, Delivered


        public virtual User? User { get; set; }

        // Shipping details
        public string? ShippingAddress { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
    }
}