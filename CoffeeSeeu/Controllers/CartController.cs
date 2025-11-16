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
    public class CartController : Controller
    {
        private const string SessionKeyCart = "CartItems";
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: /Cart
        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            return View(cart);
        }

        // GET /Cart/Add/{id}  (fallback non-AJAX)
        public async Task<IActionResult> Add(int id)
        {
            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            var cart = GetCartFromSession();
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
            SaveCartToSession(cart);

            // If AJAX header present, return JSON for backward compatibility
            // safe read of header
            var xReqHeader = Request?.Headers?["X-Requested-With"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xReqHeader) &&
                string.Equals(xReqHeader, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = true, cartCount = cart.Sum(c => c.Quantity) });
            }

            var referer = Request?.GetTypedHeaders()?.Referer?.ToString();
            if (!string.IsNullOrEmpty(referer)) return Redirect(referer);
            return RedirectToAction("Index", "Product");
        }

        // POST /Cart/AddAjax
        [HttpPost]
        public async Task<IActionResult> AddAjax([FromBody] AddAjaxRequest? req)
        {
            if (req == null || req.ProductId <= 0) return BadRequest(new { success = false });

            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == req.ProductId);
            if (product == null) return NotFound(new { success = false });

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

            SaveCartToSession(cart);
            return Json(new { success = true, cartCount = cart.Sum(c => c.Quantity) });
        }

        // POST /Cart/UpdateQuantityAjax
        [HttpPost]
        public IActionResult UpdateQuantityAjax([FromBody] UpdateQuantityRequest? req)
        {
            if (req == null || req.ProductId <= 0) return BadRequest(new { success = false });

            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(ci => ci.ProductId == req.ProductId);
            if (item == null) return NotFound(new { success = false });

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

        // POST /Cart/RemoveAjax
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

        // GET /Cart/Count
        [HttpGet]
        public IActionResult Count()
        {
            var cart = GetCartFromSession();
            return Json(new { cartCount = cart.Sum(c => c.Quantity) });
        }

        // === Checkout ===
        [HttpGet]
        public IActionResult Checkout()
        {
            var order = new Order();
            // Prefill order fields if authenticated user has profile info
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
                        // If you store address in User, set order.Address = user.Address;
                    }
                }
            }
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(Order order)
        {
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

            // If user is authenticated, ensure profile complete (server-side)
            if (User?.Identity?.IsAuthenticated == true)
            {
                var username = User.Identity?.Name;
                if (!string.IsNullOrEmpty(username))
                {
                    var user = _context.Users.FirstOrDefault(u => u.Username == username);
                    if (user != null)
                    {
                        bool complete = !string.IsNullOrWhiteSpace(user.FullName) &&
                                        !string.IsNullOrWhiteSpace(user.Phone);
                        if (!complete)
                        {
                            TempData["Error"] = "Bạn cần cập nhật thông tin cá nhân trước khi đặt hàng.";
                            return RedirectToAction("EditProfile", "Account");
                        }

                        // enforce server-side values
                        order.Username = username;
                        order.CustomerName = user.FullName;
                        order.Phone = user.Phone;
                    }
                }
            }

            order.TotalPrice = cart.Sum(ci => ci.Price * ci.Quantity);
            order.CreatedAt = DateTime.UtcNow;

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

                // clear cart in session
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

        // --- Helpers for session cart ---
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
                return new List<CartItem>();
            }
        }

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
                // ignore serialization errors for now
            }
        }
    }

    // DTOs
    public class AddAjaxRequest { public int ProductId { get; set; } }
    public class UpdateQuantityRequest { public int ProductId { get; set; } public int Quantity { get; set; } }
    public class RemoveAjaxRequest { public int ProductId { get; set; } }
}
