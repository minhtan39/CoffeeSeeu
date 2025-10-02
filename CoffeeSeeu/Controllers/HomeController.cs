using System.Diagnostics;
using CoffeeSeeu.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeSeeu.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Trang ch?
        public IActionResult Index()
        {
            return View();
        }

        // Trang s?n ph?m
        public IActionResult Products()
        {
            return View();
        }

        // Trang "V? chúng tôi"
        public IActionResult About()
        {
            return View();
        }

        // Trang liên h?
        public IActionResult Contact()
        {
            return View();
        }

        // Trang gi? hàng
        public IActionResult Cart()
        {
            return View();
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
