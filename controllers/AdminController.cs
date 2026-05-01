//adminController.cs file

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShreePerfume.Data;
using ShreePerfume.Models;
using Microsoft.AspNetCore.Http;

namespace ShreePerfume.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Security Check: Sirf Admin hi access kar sake
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        [HttpGet]
        public IActionResult AdminLogin() => View();

        [HttpPost]
        public IActionResult AdminLogin(string email, string password)
        {
            string hashedInput = HashPassword(password);
            var admin = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == hashedInput && u.Role == "Admin");

            if (admin != null)
            {
                // Consistent keys use karein
                HttpContext.Session.SetInt32("AdminId", admin.Id);
                HttpContext.Session.SetString("UserRole", "Admin");
                HttpContext.Session.SetString("UserName", admin.FullName);
                return RedirectToAction("Dashboard");
            }

            ViewBag.Error = "Invalid Admin Credentials";
            return View();
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        public IActionResult Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.TotalOrders = _context.Orders.Count();
            ViewBag.TotalSales = _context.Orders.Sum(o => (double?)o.TotalAmount) ?? 0;
            ViewBag.TotalProducts = _context.Perfumes.Count();

            // Yahan badlav karein: Sirf Customers ko count karein
            ViewBag.TotalUsers = _context.Users.Count(u => u.Role != "Admin");

            // AdminController.cs ke Dashboard method mein
            ViewBag.NewInquiries = _context.Contacts.Count(i => !i.IsRead);
            ViewBag.TotalReviews = _context.Reviews.Count();

            return View();
        }

        public IActionResult ManageProducts()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var products = _context.Perfumes.ToList();
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            // 1. Database se order dhoondein
            var order = await _context.Orders.FindAsync(id);

            if (order != null)
            {
                // 2. Status update karein
                order.Status = status;

                // 3. Database mein save karein
                _context.Update(order);
                await _context.SaveChangesAsync();
            }

            // 4. Wapas usi page par bhej dein
            return RedirectToAction("ManageOrders");
        }

        [HttpGet]
        public IActionResult AddProduct() => View();

        [HttpPost]
        public async Task<IActionResult> AddProduct(Perfume model, IFormFile ImageFile)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                // Database mein path save karna (consistent format ke liye)
                model.ImageUrl = "/images/" + fileName;
            }

            _context.Perfumes.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageProducts");
        }


        // GET: Admin/EditProduct/5
        public async Task<IActionResult> EditProduct(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var product = await _context.Perfumes.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

     
        // POST: Admin/EditProduct/5
        [HttpPost]
        public async Task<IActionResult> EditProduct(int id, Perfume model, IFormFile? ImageFile)
        {
            if (id != model.Id) return NotFound();

            try
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }
                    model.ImageUrl = "/images/" + fileName; // Naya path save ho raha hai
                }
                else
                {
                    // Agar image change nahi ki, toh purani image hi rehne dein
                    _context.Entry(model).Property(x => x.ImageUrl).IsModified = false;
                }

                _context.Update(model);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Perfumes.Any(e => e.Id == model.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction("ManageProducts");
        }
        // Delete Logic
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var product = await _context.Perfumes.FindAsync(id);
            if (product != null)
            {
                _context.Perfumes.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageProducts");
        }
        public IActionResult ManageOrders()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var orders = _context.Orders.Include(o => o.User).OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }

        public IActionResult ViewUsers()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var users = _context.Users.Where(u => u.Role != "Admin").ToList(); // Sirf customers dikhana
            return View(users);
        }

        // GET: Admin/ManageInquiries
        public IActionResult ManageInquiries()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View(_context.Contacts.OrderByDescending(c => c.SubmittedAt).ToList());
        }

        // POST: Admin/ReplyToInquiry (Back to ans logic)
        [HttpPost]
      
        public async Task<IActionResult> ReplyToInquiry(int id, string replyText)
        {
            var inquiry = await _context.Contacts.FindAsync(id);
            if (inquiry != null)
            {
                inquiry.AdminReply = replyText; // Ab ye error nahi dega
                inquiry.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageInquiries");
        }
        // GET: Admin/ManageReviews
        public IActionResult ManageReviews()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            // Review ke saath product ka naam bhi dikhane ke liye
            var reviews = _context.Reviews.OrderByDescending(r => r.Date).ToList();
            return View(reviews);
        }

        // DELETE: Review hatane ke liye
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ManageReviews");
        }


        public IActionResult Logout()
        {
            // Session ko puri tarah khatam karne ke liye
            HttpContext.Session.Clear();
            // Wapas Login page par bhej rahe hain
            return RedirectToAction("Login", "Account");
        }

        // GET: Admin/Profile
        public IActionResult Profile()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var adminId = HttpContext.Session.GetInt32("AdminId");
            var admin = _context.Users.Find(adminId);
            return View(admin);
        }

        // POST: Admin/UpdateProfile
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string fullName, string email, string newPassword)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            var admin = await _context.Users.FindAsync(adminId);

            if (admin != null)
            {
                admin.FullName = fullName;
                admin.Email = email;

                // Agar password change karna hai toh
                if (!string.IsNullOrEmpty(newPassword))
                {
                    admin.Password = newPassword; // Real project mein ise hash karna chahiye
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Profile updated successfully!";
            }

            return RedirectToAction("Profile");
        }
    }
}