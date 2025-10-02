using Microsoft.AspNetCore.Mvc;
using CoffeeSeeu.Models;

namespace CoffeeSeeu.Controllers
{
    public class CartController : Controller
    {
        private static List<CartItem> cart = new();

        public IActionResult Index() => View(cart);

        public IActionResult Add(int id)
        {
            // Demo: thêm sản phẩm giả lập
            var product = new Product { Id = id, Name = "Sản phẩm " + id, Price = 20000 };
            var item = cart.FirstOrDefault(c => c.Product.Id == id);
            if (item == null)
                cart.Add(new CartItem { Product = product, Quantity = 1 });
            else
                item.Quantity++;

            return RedirectToAction("Index");
        }

        public IActionResult Checkout() => View();
    }
}
