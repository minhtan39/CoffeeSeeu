using Microsoft.AspNetCore.Mvc;
using CoffeeSeeu.Data;
using CoffeeSeeu.Models;

namespace CoffeeSeeu.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products.ToList();
            return View(products);
        }

        // ====== PHẦN ĐÁNH GIÁ ======
        [HttpPost]
        public IActionResult RateProduct([FromBody] RateRequest req)
        {
            if (req == null || req.ProductId <= 0)
                return BadRequest();

            var product = _context.Products.FirstOrDefault(p => p.Id == req.ProductId);
            if (product == null)
                return NotFound();

            req.Rating = Math.Clamp(req.Rating, 1, 5);
            product.Rating = req.Rating;
            _context.SaveChanges();

            return Json(new { success = true, rating = req.Rating });
        }
    }

    // Lớp nhận dữ liệu JSON từ fetch
    public class RateRequest
    {
        public int ProductId { get; set; }
        public int Rating { get; set; }
    }
}
