using Microsoft.AspNetCore.Mvc;
using CoffeeSeeu.Models;

namespace CoffeeSeeu.Controllers
{
    public class ProductController : Controller
    {
        // Dữ liệu mẫu
        private static readonly List<Product> _products = new()
        {
            new Product { Id = 1, Name = "Cà phê sữa đá", Price = 30000, ImageUrl="/img/cafe1.jpg", Description="Cà phê sữa đá thơm ngon" },
            new Product { Id = 2, Name = "Espresso", Price = 40000, ImageUrl="/img/cafe2.jpg", Description="Espresso đậm đà" },
            new Product { Id = 3, Name = "Cappuccino", Price = 45000, ImageUrl="/img/cafe3.jpg", Description="Cappuccino béo ngậy" }
        };

        public IActionResult Index() => View(_products);

        public IActionResult Details(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }
    }
}
