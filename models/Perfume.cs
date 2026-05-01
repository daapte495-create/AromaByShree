namespace ShreePerfume.Models
{
    public class Perfume
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Gender { get; set; } // "Men" or "Women"
    }
}