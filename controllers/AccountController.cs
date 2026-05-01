//accountController.cs file

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShreePerfume.Data;
using ShreePerfume.Models;
using System.Text;
using System.Security.Cryptography;

namespace ShreePerfume.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login() => View();

        //[HttpPost]
        //public IActionResult Login(string email, string password)
        //{
        //    string enteredHash = HashPassword(password);
        //    var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == enteredHash);

        //    if (user != null)
        //    {
        //        HttpContext.Session.SetString("UserId", user.Id.ToString());
        //        HttpContext.Session.SetInt32("AdminId", user.Id);
        //        HttpContext.Session.SetString("UserName", user.FullName ?? "User");
        //        HttpContext.Session.SetString("UserRole", user.Role ?? "Customer");

        //        string photoPath = string.IsNullOrEmpty(user.ProfilePhotoPath) ? "default-user.png" : user.ProfilePhotoPath;
        //        HttpContext.Session.SetString("UserProfilePhoto", photoPath);

        //        if (user.Role == "Admin") return RedirectToAction("Dashboard", "Admin");
        //        return RedirectToAction("Index", "Home");
        //    }
        //    ViewBag.Error = "Invalid Login!";
        //    return View();
        //}


        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            string enteredHash = HashPassword(password);
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == enteredHash);

            if (user != null)
            {
                // --- STEP 1: LOGIN CHECK ---
                // Agar email verify nahi hai, toh login rok dein
                if (user.IsEmailVerified == false)
                {
                    ViewBag.Error = "Please verify your email first!";
                    // Demo ke liye code dobara dikha sakte hain
                    TempData["Code"] = user.VerificationCode;
                    return RedirectToAction("VerifyEmail", new { email = user.Email });
                }

                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetInt32("AdminId", user.Id);
                HttpContext.Session.SetString("UserName", user.FullName ?? "User");
                HttpContext.Session.SetString("UserRole", user.Role ?? "Customer");

                string photoPath = string.IsNullOrEmpty(user.ProfilePhotoPath) ? "default-user.png" : user.ProfilePhotoPath;
                HttpContext.Session.SetString("UserProfilePhoto", photoPath);

                if (user.Role == "Admin") return RedirectToAction("Dashboard", "Admin");
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Invalid Login!";
            return View();
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] b = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(b).Replace("-", "").ToLower();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult Register() => View();

        //[HttpPost]
        //public async Task<IActionResult> Register(RegisterViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        string hashedPassword = HashPassword(model.Password!);
        //        string fileName = "default-user.png";
        //        if (model.ProfilePhoto != null)
        //        {
        //            fileName = Guid.NewGuid().ToString() + "_" + model.ProfilePhoto.FileName;
        //            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles", fileName);
        //            using (var stream = new FileStream(path, FileMode.Create))
        //            {
        //                await model.ProfilePhoto.CopyToAsync(stream);
        //            }
        //        }

        //        var user = new User
        //        {
        //            FullName = model.FullName!,
        //            Email = model.Email!,
        //            Password = hashedPassword,
        //            Address = model.Address,
        //            PhoneNo = model.PhoneNo,
        //            Gender = model.Gender,
        //            ProfilePhotoPath = fileName,
        //            Role = "Customer"
        //        };

        //        _context.Users.Add(user);
        //        await _context.SaveChangesAsync();
        //        TempData["Success"] = "Account created successfully!";
        //        return RedirectToAction("Login");
        //    }
        //    return View(model);
        //}

        // --- STEP 2: VERIFICATION METHODS (GET & POST) ---
        [HttpGet]
        public IActionResult VerifyEmail(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(string email, string userCode)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.VerificationCode == userCode);

            if (user != null)
            {
                user.IsEmailVerified = true; // Verify kar dein
                user.VerificationCode = null; // Code clear kar dein
                await _context.SaveChangesAsync();

                TempData["Success"] = "Email Verified Successfully! You can now login.";
                return RedirectToAction("Login");
            }

            ViewBag.Error = "Invalid Verification Code!";
            ViewBag.Email = email;
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // --- STEP 3: GENERATE CODE DURING REGISTRATION ---
                string randomCode = new Random().Next(100000, 999999).ToString();

                string hashedPassword = HashPassword(model.Password!);
                string fileName = "default-user.png";
                if (model.ProfilePhoto != null)
                {
                    fileName = Guid.NewGuid().ToString() + "_" + model.ProfilePhoto.FileName;
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles", fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await model.ProfilePhoto.CopyToAsync(stream);
                    }
                }

                var user = new User
                {
                    FullName = model.FullName!,
                    Email = model.Email!,
                    Password = hashedPassword,
                    Address = model.Address,
                    PhoneNo = model.PhoneNo,
                    Gender = model.Gender,
                    ProfilePhotoPath = fileName,
                    Role = "Customer",
                    // Naye fields yahan set ho rahe hain
                    VerificationCode = randomCode,
                    IsEmailVerified = false
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Verification page par bheinjo aur code TempData mein rakho (Demo ke liye)
                TempData["Code"] = randomCode;
                return RedirectToAction("VerifyEmail", new { email = user.Email });
            }
            return View(model);
        }

        // 1. Forgot Password Page dikhane ke liye
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        // 2. Email verify karke Reset Code generate karne ke liye
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Error = "Email not found!";
                return View();
            }

            // Naya 6-digit Reset Code generate karein
            string resetCode = new Random().Next(100000, 999999).ToString();
            user.VerificationCode = resetCode; // Temporary code store karein
            await _context.SaveChangesAsync();

            TempData["ResetCode"] = resetCode; // Demo ke liye screen par dikhayenge
            return RedirectToAction("ResetPassword", new { email = email });
        }

        // 3. Reset Password Page dikhane ke liye
        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        // 4. Code verify karke naya password save karne ke liye
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email, string code, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.VerificationCode == code);

            if (user != null)
            {
                user.Password = HashPassword(newPassword); // Password hash karke save karein
                user.VerificationCode = null; // Code clear karein
                await _context.SaveChangesAsync();

                TempData["Success"] = "Password reset successful! Please login.";
                return RedirectToAction("Login");
            }

            ViewBag.Error = "Invalid Reset Code!";
            ViewBag.Email = email;
            return View();
        }

        public IActionResult Dashboard()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login");
            }

            int userId = int.Parse(userIdString);

            // 1. Orders nikaalein (Jo Model mein jayenge)
            var orders = _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            // 2. Inquiries nikaalein (Jo ViewBag mein jayenge - YEH MISSING HOGA)
            // User ki email ke basis par inquiries fetch karein
            var userEmail = _context.Users.Find(userId)?.Email;
            var inquiries = _context.Contacts
                .Where(c => c.Email == userEmail)
                .OrderByDescending(c => c.SubmittedAt)
                .ToList();

            ViewBag.Inquiries = inquiries; // Yeh line bohot zaroori hai

            return View(orders); // Orders ko model ki tarah bhejein
        }

        public IActionResult OrderDetails(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return RedirectToAction("Login");

            var order = _context.Orders.FirstOrDefault(o => o.Id == id && o.UserId == int.Parse(userId));
            if (order == null) return NotFound();

            var orderItems = _context.OrderItems.Where(oi => oi.OrderId == id).ToList();
            var perfumeIds = orderItems.Select(oi => oi.ProductId).ToList();
            ViewBag.Perfumes = _context.Perfumes.Where(p => perfumeIds.Contains(p.Id)).ToList();
            ViewBag.OrderItems = orderItems;

            return View(order);
        }

        // --- EDIT PROFILE (GET) - ISKI WAJAH SE 405 AA RAHA THA ---
        [HttpGet]
        public IActionResult EditProfile()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Users.Find(int.Parse(userId));
            if (user == null) return NotFound();

            return View(user);
        }

        // --- EDIT PROFILE (POST) ---
        [HttpPost]
        public async Task<IActionResult> EditProfile(User model, IFormFile? ProfilePhoto)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.PhoneNo = model.PhoneNo;
            user.Address = model.Address;

            if (ProfilePhoto != null && ProfilePhoto.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfilePhoto.FileName);
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");

                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string filePath = Path.Combine(uploadsFolder, fileName);

                if (!string.IsNullOrEmpty(user.ProfilePhotoPath) && user.ProfilePhotoPath != "default-user.png")
                {
                    string oldPath = Path.Combine(uploadsFolder, user.ProfilePhotoPath);
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePhoto.CopyToAsync(stream);
                }

                user.ProfilePhotoPath = fileName;
                HttpContext.Session.SetString("UserProfilePhoto", fileName);
            }

            _context.Update(user);
            await _context.SaveChangesAsync();
            HttpContext.Session.SetString("UserName", user.FullName);

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Users.Find(int.Parse(userId));
            if (user != null && user.Password == HashPassword(model.OldPassword))
            {
                if (model.NewPassword == model.ConfirmPassword)
                {
                    user.Password = HashPassword(model.NewPassword);
                    _context.Update(user);
                    _context.SaveChanges();
                    TempData["Success"] = "Password changed successfully!";
                }
                else { TempData["Error"] = "Passwords do not match!"; }
            }

            else { TempData["Error"] = "Old password is incorrect!"; }

            return RedirectToAction("EditProfile");
        }

        public IActionResult DeleteAccount()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = _context.Users.Find(int.Parse(userId));
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
                HttpContext.Session.Clear();
                return RedirectToAction("Register");
            }
            return RedirectToAction("Dashboard");
        }
    }
}