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
            // Demo: thêm sản phẩm giả lập (sau này lấy từ DB)
            var product = new Product { Id = id, Name = "Sản phẩm " + id, Price = 20000 };
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
