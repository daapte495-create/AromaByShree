//ViewComponents/ CartCountViewComponent.cs file

using Microsoft.AspNetCore.Mvc;
using ShreePerfume.Models;
using System.Text.Json;

namespace ShreePerfume.ViewComponents
{
    public class CartCountViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var sessionData = HttpContext.Session.GetString("Cart");
            int count = 0;

            if (!string.IsNullOrEmpty(sessionData))
            {
                var cart = JsonSerializer.Deserialize<List<CartItem>>(sessionData);
                // Sum up all quantities in the cart
                count = cart?.Sum(x => x.Quantity) ?? 0;
            }

            return View(count);
        }
    }
}
