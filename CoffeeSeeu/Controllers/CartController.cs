using Microsoft.AspNetCore.Mvc;
using CoffeeSeeu.Models;
using CoffeeSeeu.Data;
using System.Linq;
using System.Collections.Generic;

namespace CoffeeSeeu.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private static List<CartItem> cart = new();

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View(cart);

        public IActionResult Add(int id)
        {
            // 🔹 Lấy sản phẩm thật từ database
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound();

            // 🔹 Kiểm tra xem sản phẩm đã có trong giỏ chưa
            var item = cart.FirstOrDefault(c => c.Product != null && c.Product.Id == id);

            if (item == null)
                cart.Add(new CartItem { Product = product, Quantity = 1 });
            else
                item.Quantity++;

            return RedirectToAction("Index");
        }

        // GET: hiển thị form Checkout
        [HttpGet]
        public IActionResult Checkout()
        {
            return View(new Order());
        }

        // POST: nhận dữ liệu từ form Checkout
        [HttpPost]
        public IActionResult Checkout(Order order)
        {
            if (!ModelState.IsValid)
            {
                return View(order); // Nếu sai dữ liệu thì hiển thị lại form
            }

            // Sau này sẽ lưu vào database. Tạm thời chỉ xóa giỏ hàng
            cart.Clear();

            ViewBag.Message = "Đặt hàng thành công! Cảm ơn bạn đã mua hàng.";
            return View("OrderSuccess", order); // Chuyển sang view thông báo thành công
        }
    }
}
