using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CoffeeSeeu.Data;
using CoffeeSeeu.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeSeeu.Controllers
{
    /* =========================================================================
     * Mục đích   : Xử lý giỏ hàng (Thêm, Cập nhật, Xóa, Checkout) và lưu vào Session
     * ========================================================================= */

    /// <summary>
    /// Controller xử lý chức năng giỏ hàng:
    /// - Index: hiển thị giỏ hàng
    /// - Add/AddAjax: thêm sản phẩm (AJAX và non-AJAX)
    /// - UpdateQuantityAjax / RemoveAjax: cập nhật/xóa item trong giỏ qua AJAX
    /// - Checkout: tạo đơn hàng từ giỏ hàng
    /// Dữ liệu giỏ hàng được lưu trong Session dưới key "CartItems" (JSON).
    /// </summary>
    public class CartController : Controller
    {
        // Khóa lưu giỏ hàng trong session
        private const string SessionKeyCart = "CartItems";
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor: nhận ApplicationDbContext qua DI để truy vấn Products, Orders...
        /// </summary>
        public CartController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Mục đích: Hiển thị trang giỏ hàng
        /// Trả về: View với model List<CartItem> (lấy từ Session)
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            // Lấy giỏ hàng từ session (nếu chưa có -> list rỗng)
            var cart = GetCartFromSession();
            return View(cart);
        }

        /// <summary>
        /// Mục đích: Thêm sản phẩm vào giỏ (non-AJAX fallback)
        /// Tham số:
        ///   - id: mã sản phẩm
        /// Trả về:
        ///   - Nếu là AJAX: JSON { success, cartCount }
        ///   - Nếu non-AJAX: redirect về referer hoặc Product Index
        /// </summary>
        public async Task<IActionResult> Add(int id)
        {
            // Lấy sản phẩm từ DB (AsNoTracking vì không cần thay đổi)
            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            // Lấy giỏ hiện tại
            var cart = GetCartFromSession();

            // Nếu item chưa có thì thêm mới, nếu có thì tăng quantity
            var item = cart.FirstOrDefault(ci => ci.ProductId == id);
            if (item == null)
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = 1,
                    ImageUrl = product.ImageUrl
                });
            }
            else
            {
                item.Quantity++;
            }

            // Lưu lại session
            SaveCartToSession(cart);

            // Kiểm tra header AJAX: trả JSON nếu là AJAX
            var xReqHeader = Request?.Headers?["X-Requested-With"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xReqHeader) &&
                string.Equals(xReqHeader, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = true, cartCount = cart.Sum(c => c.Quantity) });
            }

            // Nếu không phải AJAX, chuyển về trang trước (referer) nếu có, ngược lại về Product index
            var referer = Request?.GetTypedHeaders()?.Referer?.ToString();
            if (!string.IsNullOrEmpty(referer)) return Redirect(referer);
            return RedirectToAction("Index", "Product");
        }

        /// <summary>
        /// Mục đích: Thêm sản phẩm vào giỏ bằng AJAX (nhận JSON body)
        /// Tham số:
        ///   - req: { ProductId }
        /// Trả về: JSON { success, cartCount }
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddAjax([FromBody] AddAjaxRequest? req)
        {
            // Validate request
            if (req == null || req.ProductId <= 0) return BadRequest(new { success = false });

            // Lấy sản phẩm
            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == req.ProductId);
            if (product == null) return NotFound(new { success = false });

            // Thêm/tăng số lượng
            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(ci => ci.ProductId == req.ProductId);
            if (item == null)
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = 1,
                    ImageUrl = product.ImageUrl
                });
            }
            else
            {
                item.Quantity++;
            }

            // Lưu session và trả JSON
            SaveCartToSession(cart);
            return Json(new { success = true, cartCount = cart.Sum(c => c.Quantity) });
        }

        /// <summary>
        /// Mục đích: Cập nhật số lượng cho 1 item (AJAX)
        /// Tham số:
        ///   - req: { ProductId, Quantity }
        /// Trả về: JSON thông tin trạng thái và số lượng tổng
        /// </summary>
        [HttpPost]
        public IActionResult UpdateQuantityAjax([FromBody] UpdateQuantityRequest? req)
        {
            if (req == null || req.ProductId <= 0) return BadRequest(new { success = false });

            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(ci => ci.ProductId == req.ProductId);
            if (item == null) return NotFound(new { success = false });

            // Nếu quantity <= 0 thì xoá item khỏi giỏ, ngược lại cập nhật
            if (req.Quantity <= 0)
            {
                cart.Remove(item);
            }
            else
            {
                item.Quantity = req.Quantity;
            }

            SaveCartToSession(cart);

            return Json(new
            {
                success = true,
                cartCount = cart.Sum(c => c.Quantity),
                itemQuantity = req.Quantity <= 0 ? 0 : req.Quantity
            });
        }

        /// <summary>
        /// Mục đích: Xóa 1 sản phẩm khỏi giỏ (AJAX)
        /// Tham số:
        ///   - req: { ProductId }
        /// Trả về: JSON { success, cartCount }
        /// </summary>
        [HttpPost]
        public IActionResult RemoveAjax([FromBody] RemoveAjaxRequest? req)
        {
            if (req == null || req.ProductId <= 0) return BadRequest(new { success = false });

            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(ci => ci.ProductId == req.ProductId);
            if (item != null)
            {
                cart.Remove(item);
                SaveCartToSession(cart);
            }

            return Json(new { success = true, cartCount = cart.Sum(c => c.Quantity) });
        }

        /// <summary>
        /// Mục đích: Lấy số lượng tổng trong giỏ (dùng cho badge)
        /// Trả về: JSON { cartCount }
        /// </summary>
        [HttpGet]
        public IActionResult Count()
        {
            var cart = GetCartFromSession();
            return Json(new { cartCount = cart.Sum(c => c.Quantity) });
        }

        /// <summary>
        /// Mục đích: Hiển thị form Checkout (GET)
        /// - Nếu user đã đăng nhập và có thông tin, sẽ tiền điền một số trường
        /// </summary>
        [HttpGet]
        public IActionResult Checkout()
        {
            var order = new Order();

            // Nếu user đã đăng nhập thì tiền điền thông tin cơ bản từ user profile
            if (User?.Identity?.IsAuthenticated == true)
            {
                var username = User.Identity?.Name;
                if (!string.IsNullOrEmpty(username))
                {
                    var user = _context.Users.FirstOrDefault(u => u.Username == username);
                    if (user != null)
                    {
                        order.Username = username;
                        order.CustomerName = user.FullName;
                        order.Phone = user.Phone;
                        // Nếu lưu address trong User thì gán order.Address = user.Address;
                    }
                }
            }

            return View(order);
        }

        /// <summary>
        /// Mục đích: Xử lý Checkout (POST) - lưu Order và OrderItems vào DB
        /// Tham số:
        ///   - order: model Order nhận từ form
        /// Trả về:
        ///   - Nếu thành công: view OrderSuccess
        ///   - Nếu lỗi hoặc giỏ rỗng: trả về View(order) và hiển thị lỗi
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(Order order)
        {
            // Kiểm tra model hợp lệ
            if (!ModelState.IsValid)
            {
                return View(order);
            }

            var cart = GetCartFromSession();
            if (cart.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Giỏ hàng rỗng.");
                return View(order);
            }

            // Nếu user đã đăng nhập, kiểm tra profile đã hoàn thiện (server-side)
            if (User?.Identity?.IsAuthenticated == true)
            {
                var username = User.Identity?.Name;
                if (!string.IsNullOrEmpty(username))
                {
                    var user = _context.Users.FirstOrDefault(u => u.Username == username);
                    if (user != null)
                    {
                        // Yêu cầu tối thiểu: FullName và Phone phải có
                        bool complete = !string.IsNullOrWhiteSpace(user.FullName) &&
                                        !string.IsNullOrWhiteSpace(user.Phone);
                        if (!complete)
                        {
                            TempData["Error"] = "Bạn cần cập nhật thông tin cá nhân trước khi đặt hàng.";
                            return RedirectToAction("EditProfile", "Account");
                        }

                        // Ghi nhận giá trị server-side để tránh forgery
                        order.Username = username;
                        order.CustomerName = user.FullName;
                        order.Phone = user.Phone;
                    }
                }
            }

            // Tính tổng và gán thời gian tạo
            order.TotalPrice = cart.Sum(ci => ci.Price * ci.Quantity);
            order.CreatedAt = DateTime.UtcNow;

            // Lưu Order và OrderItems trong transaction để đảm bảo atomic
            using var tx = _context.Database.BeginTransaction();
            try
            {
                _context.Orders.Add(order);
                _context.SaveChanges();

                foreach (var ci in cart)
                {
                    var oi = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = ci.ProductId,
                        ProductName = ci.Name ?? string.Empty,
                        UnitPrice = ci.Price,
                        Quantity = ci.Quantity
                    };
                    _context.OrderItems.Add(oi);
                }

                _context.SaveChanges();
                tx.Commit();

                // Clear cart trong session sau khi đặt hàng thành công
                SaveCartToSession(new List<CartItem>());

                ViewBag.Message = "Đặt hàng thành công! Mã đơn: " + order.Id;
                return View("OrderSuccess", order);
            }
            catch
            {
                tx.Rollback();
                ModelState.AddModelError(string.Empty, "Lỗi khi lưu đơn hàng, vui lòng thử lại.");
                return View(order);
            }
        }

        /* =========================
         * Helpers: Lưu / Lấy giỏ hàng từ Session
         * Dữ liệu lưu dạng JSON (List<CartItem>)
         * ========================= */

        /// <summary>
        /// Lấy danh sách CartItem từ Session
        /// Trả về: List<CartItem> (không bao giờ trả null)
        /// </summary>
        private List<CartItem> GetCartFromSession()
        {
            try
            {
                var session = HttpContext?.Session;
                if (session == null) return new List<CartItem>();

                var json = session.GetString(SessionKeyCart);
                if (string.IsNullOrEmpty(json)) return new List<CartItem>();

                var list = JsonSerializer.Deserialize<List<CartItem>>(json);
                return list ?? new List<CartItem>();
            }
            catch
            {
                // Nếu lỗi (json hỏng, deserialize lỗi...), trả list rỗng để tránh crash
                return new List<CartItem>();
            }
        }

        /// <summary>
        /// Lưu danh sách CartItem vào Session ở dạng JSON
        /// </summary>
        private void SaveCartToSession(List<CartItem> cart)
        {
            try
            {
                var session = HttpContext?.Session;
                if (session == null) return;

                var json = JsonSerializer.Serialize(cart ?? new List<CartItem>());
                session.SetString(SessionKeyCart, json);
            }
            catch
            {
                // Bắt và bỏ qua lỗi serialize/IO để không làm gián đoạn flow
            }
        }
    }

    // =========================
    // DTOs cho các request AJAX
    // =========================

    /// <summary>
    /// DTO cho AddAjax: chỉ cần ProductId
    /// </summary>
    public class AddAjaxRequest { public int ProductId { get; set; } }

    /// <summary>
    /// DTO cho UpdateQuantityAjax
    /// </summary>
    public class UpdateQuantityRequest { public int ProductId { get; set; } public int Quantity { get; set; } }

    /// <summary>
    /// DTO cho RemoveAjax
    /// </summary>
    public class RemoveAjaxRequest { public int ProductId { get; set; } }
}
