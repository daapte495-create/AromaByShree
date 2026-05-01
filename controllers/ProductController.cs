//ProductContoller.cs file

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShreePerfume.Data;
using ShreePerfume.Models;
using System.Text.Json;

namespace ShreePerfume.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- SHOP PAGE ---
        [HttpGet]
        public IActionResult Shop(string? search, string? gender)
        {

            ViewBag.SelectedGender = gender;

            var query = _context.Perfumes.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name!.Contains(search) || p.Description!.Contains(search));
            }

            if (!string.IsNullOrEmpty(gender))
            {
                query = query.Where(p => p.Gender == gender);
            }

            return View(query.ToList());
        }

        // --- DETAILS PAGE ---
        [HttpGet]
        public IActionResult Details(int id)
        {
            var product = _context.Perfumes.Find(id);
            if (product == null) return NotFound();

            // Reviews ko har baar load karna zaroori hai
            var reviews = _context.Reviews
                                  .Where(r => r.ProductId == id)
                                  .OrderByDescending(r => r.Date)
                                  .ToList();

            ViewBag.Reviews = reviews;
            return View(product);
        }

        // --- ADD TO CART ---
        [HttpPost]
        public IActionResult AddToCart(int id, int quantity)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                TempData["Error"] = "Please login to add items to your cart.";
                return RedirectToAction("Login", "Account");
            }

            AddToCartLogic(id, quantity);
            return RedirectToAction("Cart");
        }

        private void AddToCartLogic(int id, int quantity)
        {
            var product = _context.Perfumes.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                var sessionData = HttpContext.Session.GetString("Cart");
                var cart = string.IsNullOrEmpty(sessionData)
                           ? new List<CartItem>()
                           : JsonSerializer.Deserialize<List<CartItem>>(sessionData)!;

                var existingItem = cart.FirstOrDefault(c => c.ProductId == id);
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId = id,
                        ProductName = product.Name!,
                        Price = product.Price,
                        Quantity = quantity,
                        ImageUrl = product.ImageUrl!
                    });
                }
                HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
            }
        }

        // --- CART VIEW ---
        public IActionResult Cart()
        {
            var sessionData = HttpContext.Session.GetString("Cart");
            var cart = string.IsNullOrEmpty(sessionData)
                       ? new List<CartItem>()
                       : JsonSerializer.Deserialize<List<CartItem>>(sessionData)!;

            return View(cart);
        }

        public IActionResult RemoveFromCart(int id)
        {
            var sessionData = HttpContext.Session.GetString("Cart");
            if (!string.IsNullOrEmpty(sessionData))
            {
                var cart = JsonSerializer.Deserialize<List<CartItem>>(sessionData)!;
                var itemToRemove = cart.FirstOrDefault(x => x.ProductId == id);
                if (itemToRemove != null)
                {
                    cart.Remove(itemToRemove);
                }
                HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
            }
            return RedirectToAction("Cart");
        }

        public IActionResult UpdateQuantity(int id, int amount)
        {
            var sessionData = HttpContext.Session.GetString("Cart");
            if (!string.IsNullOrEmpty(sessionData))
            {
                var cart = JsonSerializer.Deserialize<List<CartItem>>(sessionData)!;
                var item = cart.FirstOrDefault(x => x.ProductId == id);

                if (item != null)
                {
                    item.Quantity += amount;
                    if (item.Quantity <= 0) cart.Remove(item);
                }
                HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
            }
            return RedirectToAction("Cart");
        }

        // --- WISHLIST ---
        [HttpPost]
        public IActionResult AddToWishlist(int id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                TempData["Error"] = "Please login to save items to your wishlist.";
                return RedirectToAction("Login", "Account");
            }

            var sessionData = HttpContext.Session.GetString("Wishlist");
            var wishlist = string.IsNullOrEmpty(sessionData)
                           ? new List<int>()
                           : JsonSerializer.Deserialize<List<int>>(sessionData)!;

            if (!wishlist.Contains(id)) wishlist.Add(id);

            HttpContext.Session.SetString("Wishlist", JsonSerializer.Serialize(wishlist));
            return RedirectToAction("Wishlist");
        }

        public IActionResult Wishlist()
        {
            var sessionData = HttpContext.Session.GetString("Wishlist");
            var wishlistIds = string.IsNullOrEmpty(sessionData)
                              ? new List<int>()
                              : JsonSerializer.Deserialize<List<int>>(sessionData)!;

            var products = _context.Perfumes.Where(p => wishlistIds.Contains(p.Id)).ToList();
            return View(products);
        }

        public IActionResult RemoveFromWishlist(int id)
        {
            var sessionData = HttpContext.Session.GetString("Wishlist");
            if (!string.IsNullOrEmpty(sessionData))
            {
                var wishlist = JsonSerializer.Deserialize<List<int>>(sessionData)!;
                if (wishlist.Contains(id)) wishlist.Remove(id);
                HttpContext.Session.SetString("Wishlist", JsonSerializer.Serialize(wishlist));
            }
            return RedirectToAction("Wishlist");
        }

        // --- SHIPPING & ORDER ---
        [HttpGet]
        public IActionResult Shipping()
        {
            // Security check: Agar user login nahi hai toh shipping page mat dikhao
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(string FullName, string PhoneNumber, string Address)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            var sessionData = HttpContext.Session.GetString("Cart");

            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
            if (string.IsNullOrEmpty(sessionData)) return RedirectToAction("Shop");

            var cart = JsonSerializer.Deserialize<List<CartItem>>(sessionData)!;
            int userId = int.Parse(userIdString);

            // 1. Order save karna
            var newOrder = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = cart.Sum(item => item.Price * item.Quantity),
                Status = "Pending",
                ShippingAddress = Address,
                Phone = PhoneNumber
            };

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            // 2. OrderItems save karna
            foreach (var item in cart)
            {
                var orderDetail = new OrderItem
                {
                    OrderId = newOrder.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                };
                _context.OrderItems.Add(orderDetail);
            }

            await _context.SaveChangesAsync();

            // 3. Cleanup
            HttpContext.Session.Remove("Cart");

            return View("OrderSuccess");
        }

        [HttpPost]
        public IActionResult BuyNow(int id, int quantity)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                TempData["Error"] = "Please login to buy products.";
                return RedirectToAction("Login", "Account");
            }

            AddToCartLogic(id, quantity);
            return RedirectToAction("Shipping");
        }

       
        [HttpPost]
        public IActionResult PostReview(int productId, int rating, string comment)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var review = new Review
            {
                ProductId = productId,
                UserId = int.Parse(userId),
                UserName = HttpContext.Session.GetString("UserName") ?? "Anonymous",
                Rating = rating,
                Comment = comment,
                Date = DateTime.Now
            };

            try
            {
                _context.Reviews.Add(review);
                _context.SaveChanges();
                TempData["Success"] = "Review submitted successfully!";
                return RedirectToAction("Details", new { id = productId });
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException?.Message;
                TempData["Error"] = "Database Error: " + innerException;
                return RedirectToAction("Details", new { id = productId });
            }
        }
    }
}