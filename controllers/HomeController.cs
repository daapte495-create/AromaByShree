using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShreePerfume.Data;
using ShreePerfume.Models;
using System.Diagnostics;

namespace ShreePerfume.Controllers
{
    public class HomeController : Controller
    {
        // 1. Dono fields ko pehle declare karein
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        // 2. Sirf EK constructor banayein jo dono ko accept kare
        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var products = new List<Perfume>
            {
                new Perfume { Id = 1, Name = "Royal Oud", Price = 4500, Description = "Rich Agarwood & Musk", ImageUrl ="/images/perfume.jpg" },
                new Perfume { Id = 2, Name = "Velvet Rose", Price = 3200, Description = "Fresh Bulgarian Rose", ImageUrl = "/images/perfume.jpg"},
                new Perfume { Id = 3, Name = "Oceanic Mist", Price = 2800, Description = "Sea Salt & Citrus", ImageUrl = "/images/perfume.jpg" },
                new Perfume { Id = 4, Name = "Golden Amber", Price = 5100, Description = "Warm Amber & Vanilla", ImageUrl = "/images/perfume.jpg" },
                new Perfume { Id = 5, Name = "Rose Patel", Price = 5100, Description = "Rose elegance", ImageUrl = "/images/perfume.jpg" }
            };

            return View(products);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contact(ContactForm model)
        {
            if (ModelState.IsValid)
            {
                _context.Contacts.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thank you! Your message has been sent successfully.";
                return RedirectToAction("Contact");
            }
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}