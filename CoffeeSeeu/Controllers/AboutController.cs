using Microsoft.AspNetCore.Mvc;
using CoffeeSeeu.Data;

namespace CoffeeSeeu.Controllers
{
    public class AboutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AboutController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Lấy album "Không gian quán"
            var spaceAlbum = _context.Albums
                .Where(a => a.Name == "Không gian quán")
                .Select(a => a.Images.Select(i => new
                {
                    i.ImagePath,
                    i.Description
                }))
                .FirstOrDefault();

            // Lấy album "Đội ngũ"
            var staffAlbum = _context.Albums
                .Where(a => a.Name == "Đội ngũ")
                .Select(a => a.Images.Select(i => new
                {
                    i.ImagePath,
                    i.Description,
                    Role = i.Album.Description // có thể dùng Description để chứa chức vụ
                }))
                .FirstOrDefault();

            ViewBag.SpaceImages = spaceAlbum;
            ViewBag.StaffImages = staffAlbum;

            return View();
        }
    }
}
