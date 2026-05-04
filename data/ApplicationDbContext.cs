using Microsoft.EntityFrameworkCore;
using ShreePerfume.Models;

namespace ShreePerfume.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Ye line database mein 'Perfumes' naam ki table banati hai
        //(to make all table )
        public DbSet<Perfume> Perfumes { get; set; }

        // user table banati hain
        public DbSet<User> Users { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }
      
        public DbSet<ContactForm> Contacts { get; set; }
        public DbSet<Review> Reviews { get; set; }

        //(to make all table )


        /// <param name="modelBuilder"></param>

        // Is method ke andar hum 'Seed Data' daalte hain
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Review>().ToTable("Review");


            // Ye data automatic database mein chala jayega
            modelBuilder.Entity<Perfume>().HasData(
                new Perfume
                {
                    Id = 1,
                    Name = "Royal Oud",
                    Price = 4500,
                    Gender = "Men",
                    Description = "Deep Wood & Arabian Musk. A majestic scent for formal evenings.",
                    ImageUrl = "/images/perfume.jpg"
                },
                new Perfume
                {
                    Id = 2,
                    Name = "Velvet Rose",
                    Price = 3200,
                    Gender = "Women",
                    Description = "Fresh Blooming Red Rose. Elegant and timelessly feminine.",
                    ImageUrl = "/images/perfume.webp"
                },
                new Perfume
                {
                    Id = 3,
                    Name = "Midnight Blue",
                    Price = 3800,
                    Gender = "Men",
                    Description = "Cool Citrus & Ocean Breeze. Perfect for an active daily lifestyle.",
                    ImageUrl = "/images/perfume.jpg"
                },
                new Perfume
                {
                    Id = 4,
                    Name = "Golden Jasmine",
                    Price = 4100,
                    Gender = "Women",
                    Description = "Sweet Floral & White Honey. A warm fragrance.",
                    ImageUrl = "/images/perfume.webp"
                },
                new Perfume
                {
                    Id = 5,
                    Name = "Oasis Mist",
                    Price = 2800,
                    Gender = "Women",
                    Description = "Fresh and summery.",
                    ImageUrl = "/images/about.jpg"
                }
            );
        }
    }
}