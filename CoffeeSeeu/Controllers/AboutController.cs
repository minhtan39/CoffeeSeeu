using Microsoft.AspNetCore.Mvc;
using CoffeeSeeu.Data;

namespace CoffeeSeeu.Controllers
{
    public class AboutController : Controller
    {
        // Khai báo DbContext để truy vấn dữ liệu từ database
        private readonly ApplicationDbContext _context;

        // Constructor nhận DbContext thông qua Dependency Injection
        public AboutController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang giới thiệu (About)
        public IActionResult Index()
        {
            /* ============================================================
             * LẤY DỮ LIỆU ALBUM HÌNH ẢNH
             * - Lấy ảnh không gian quán
             * - Lấy ảnh đội ngũ nhân viên
             * - Trả ra ViewBag để hiển thị trên trang About
             * ============================================================ */

            // 1) Lấy album "Không gian quán" + danh sách ảnh
            var spaceAlbum = _context.Albums
                .Where(a => a.Name == "Không gian quán")
                .Select(a => a.Images.Select(i => new
                {
                    i.ImagePath,      // đường dẫn ảnh
                    i.Description     // mô tả ảnh
                }))
                .FirstOrDefault();     // lấy 1 album đầu tiên (đúng tên)

            // 2) Lấy album "Đội ngũ" + danh sách ảnh
            var staffAlbum = _context.Albums
                .Where(a => a.Name == "Đội ngũ")
                .Select(a => a.Images.Select(i => new
                {
                    i.ImagePath,      // đường dẫn ảnh
                    i.Description,    // mô tả ảnh
                    Role = i.Album.Description // Dùng mô tả album để lưu vai trò (nếu có)
                }))
                .FirstOrDefault();

            // Gửi dữ liệu sang View
            ViewBag.SpaceImages = spaceAlbum;
            ViewBag.StaffImages = staffAlbum;

            return View();
        }
    }
}
