using System.Diagnostics;
using CoffeeSeeu.Models;
using CoffeeSeeu.Data;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeSeeu.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Trang chủ
        public IActionResult Index()
        {
            // Lấy 4 sản phẩm đầu tiên từ database để hiển thị
            var featuredProducts = _context.Products.Take(4).ToList();
            return View(featuredProducts);
        }

        public IActionResult About() => View();

        public IActionResult Contact() => View();

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
