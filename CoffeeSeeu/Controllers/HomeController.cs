using System.Diagnostics;
using CoffeeSeeu.Models;
using CoffeeSeeu.Data;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeSeeu.Controllers
{
    /* =======================================================================
     * File       : Controllers/HomeController.cs
     * Mục đích   : Xử lý trang chủ và các trang giới thiệu cơ bản của website
     * Người tạo  : [Tên sinh viên]
     * Ngày tạo   : 2025-11-17
     * Ghi chú    : Chỉ bổ sung chú thích – không thay đổi logic nguồn
     * ======================================================================= */

    public class HomeController : Controller
    {
        // Logger để ghi log hệ thống
        private readonly ILogger<HomeController> _logger;

        // DbContext để lấy dữ liệu sản phẩm hiển thị ở trang chủ
        private readonly ApplicationDbContext _context;

        // Constructor nhận tham số qua Dependency Injection
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Trang chủ (Home)
        /// - Lấy top 4 sản phẩm có Rating cao nhất để hiển thị mục "Sản phẩm nổi bật"
        /// </summary>
        public IActionResult Index()
        {
            // Lấy danh sách sản phẩm có đánh giá cao nhất
            var featuredProducts = _context.Products
                .OrderByDescending(p => p.Rating)
                .Take(4)                // lấy đúng 4 item
                .ToList();

            // Truyền list sản phẩm sang View
            return View(featuredProducts);
        }

        /// <summary>
        /// Trang giới thiệu (About)
        /// </summary>
        public IActionResult About() => View();

        /// <summary>
        /// Trang liên hệ (Contact)
        /// </summary>
        public IActionResult Contact() => View();

        /// <summary>
        /// Trang chính sách bảo mật
        /// </summary>
        public IActionResult Privacy() => View();

        /// <summary>
        /// Trang hiển thị lỗi hệ thống (mặc định của ASP.NET)
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                }
            );
        }
    }
}
