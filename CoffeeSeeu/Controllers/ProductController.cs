using Microsoft.AspNetCore.Mvc;
using CoffeeSeeu.Models;

namespace CoffeeSeeu.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            // Tạm thời mock dữ liệu sản phẩm (chưa dùng DB)
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Cà phê hạt Arabica", Price = 120000, ImageUrl = "/images/product1.jpg" },
                new Product { Id = 2, Name = "Cà phê hạt Robusta", Price = 95000, ImageUrl = "/images/product2.jpg" },
                new Product { Id = 3, Name = "Cà phê sữa đá", Price = 45000, ImageUrl = "/images/product3.jpg" },
                new Product { Id = 4, Name = "Cà phê pha phin", Price = 55000, ImageUrl = "/images/product4.jpg" }
            };

            return View(products);
        }
    }
}
