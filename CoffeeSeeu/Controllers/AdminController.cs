using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoffeeSeeu.Data;
using CoffeeSeeu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace CoffeeSeeu.Controllers
{
    /* =========================================================================
     * Mục đích   : Xử lý chức năng quản trị (Dashboard, quản lý sản phẩm, quản lý người dùng)
     * ========================================================================= */

    // Chỉ cho phép user có Role = "Admin" truy cập controller này
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // DbContext để thao tác với database (Products, Users, Orders...)
        private readonly ApplicationDbContext _context;
        // IWebHostEnvironment để lưu file ảnh (upload)
        private readonly IWebHostEnvironment _env;

        /// <summary>
        /// Constructor: nhận DI các service cần thiết
        /// </summary>
        public AdminController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        /* =========================
         * Dashboard (Admin home)
         * - Hiển thị số liệu tóm tắt: số sản phẩm, số người dùng, số đơn
         * ========================= */
        public IActionResult Index()
        {
            // Dùng null-conditional để tránh trường hợp test _context.* = null
            ViewBag.ProductCount = _context.Products?.Count() ?? 0;
            ViewBag.UserCount = _context.Users?.Count() ?? 0;
            ViewBag.OrderCount = _context.Orders?.Count() ?? 0;
            return View();
        }

        /* =========================
         * Products - Danh sách & tìm kiếm
         * GET: /Admin/Products
         * - q: từ khoá tìm theo tên
         * - page, pageSize: phân trang đơn giản
         * ========================= */
        public async Task<IActionResult> Products(string q, int page = 1, int pageSize = 20)
        {
            // Lấy queryable an toàn (tránh null)
            var query = _context.Products?.AsQueryable() ?? Enumerable.Empty<Product>().AsQueryable();

            // Nếu có từ khoá, lọc theo tên (bảo vệ null cho p.Name)
            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(p => p.Name != null && p.Name.Contains(q));
            }

            // Phân trang cơ bản: sắp xếp theo Id giảm dần, skip & take
            var items = await query
                .OrderByDescending(p => p.Id)
                .Skip((Math.Max(1, page) - 1) * Math.Max(1, pageSize))
                .Take(pageSize)
                .ToListAsync();

            // Lưu lại vài thông tin cho view sử dụng (giữ trạng thái tìm kiếm, trang)
            ViewBag.Query = q;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            // Trả view ở thư mục Views/Admin/Products/Index.cshtml
            return View("Products/Index", items);
        }

        /* =========================
         * CreateProduct (GET)
         * - Trả về form tạo sản phẩm mới
         * ========================= */
        public IActionResult CreateProduct()
        {
            // Trả về view tạo với model Product rỗng (giúp binding form)
            return View("Products/Create", new Product());
        }

        /* =========================
         * CreateProduct (POST)
         * - Xử lý lưu sản phẩm mới và upload ảnh (nếu có)
         * ========================= */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product model, IFormFile? ImageFile)
        {
            // Nếu model không hợp lệ -> trả lại form cùng dữ liệu để hiển thị lỗi
            if (!ModelState.IsValid)
            {
                return View("Products/Create", model);
            }

            // Nếu có upload ảnh, kiểm tra extension và lưu file
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var ext = Path.GetExtension(ImageFile.FileName) ?? "";
                // Dùng HashSet với OrdinalIgnoreCase để so sánh extension an toàn
                var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("", "Chỉ chấp nhận file ảnh (jpg, png, webp).");
                    return View("Products/Create", model);
                }

                // Tạo tên file duy nhất và thư mục lưu
                var fileName = $"{Guid.NewGuid()}{ext}";
                var saveFolder = Path.Combine(_env.WebRootPath, "img", "products");
                Directory.CreateDirectory(saveFolder);
                var savePath = Path.Combine(saveFolder, fileName);

                // Ghi file lên server
                using (var fs = new FileStream(savePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fs);
                }

                // Gán đường dẫn file vào model (dùng path bắt đầu bằng '/')
                model.ImageUrl = $"/img/products/{fileName}";
            }

            // Lưu sản phẩm vào DB
            _context.Products.Add(model);
            await _context.SaveChangesAsync();

            // Thông báo thành công và chuyển về danh sách sản phẩm
            TempData["Success"] = "Thêm sản phẩm thành công";
            return RedirectToAction(nameof(Products));
        }

        /* =========================
         * EditProduct (GET)
         * - Trả về form sửa sản phẩm theo id
         * ========================= */
        public async Task<IActionResult> EditProduct(int id)
        {
            var p = await _context.Products!.FindAsync(id);
            if (p == null) return NotFound();
            return View("Products/Edit", p);
        }

        /* =========================
         * EditProduct (POST)
         * - Cập nhật thông tin sản phẩm, xử lý upload ảnh mới (tuỳ chọn)
         * ========================= */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product model, IFormFile? ImageFile)
        {
            var p = await _context.Products!.FindAsync(id);
            if (p == null) return NotFound();

            // Nếu model không hợp lệ -> trả lại form edit với dữ liệu nhập
            if (!ModelState.IsValid) return View("Products/Edit", model);

            // Cập nhật các trường cơ bản
            p.Name = model.Name;
            p.Description = model.Description;
            p.Price = model.Price;
            p.Rating = model.Rating;

            // Xử lý nếu có ảnh mới
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var ext = Path.GetExtension(ImageFile.FileName) ?? "";
                var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("", "Chỉ chấp nhận file ảnh (jpg, png, webp).");
                    return View("Products/Edit", model);
                }

                // Lưu file mới
                var fileName = $"{Guid.NewGuid()}{ext}";
                var saveFolder = Path.Combine(_env.WebRootPath, "img", "products");
                Directory.CreateDirectory(saveFolder);
                var savePath = Path.Combine(saveFolder, fileName);
                using (var fs = new FileStream(savePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fs);
                }

                // Cố gắng xóa file cũ (nếu không phải là ảnh mặc định)
                try
                {
                    if (!string.IsNullOrEmpty(p.ImageUrl) && !p.ImageUrl.Contains("default", StringComparison.OrdinalIgnoreCase))
                    {
                        var old = Path.Combine(_env.WebRootPath, p.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(old)) System.IO.File.Delete(old);
                    }
                }
                catch
                {
                    // Nếu xóa thất bại thì bỏ qua (không làm vỡ flow)
                }

                // Cập nhật đường dẫn ảnh mới
                p.ImageUrl = $"/img/products/{fileName}";
            }

            // Lưu thay đổi vào DB
            _context.Products.Update(p);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật sản phẩm thành công";
            return RedirectToAction(nameof(Products));
        }

        /* =========================
         * DeleteProduct (POST)
         * - Xóa sản phẩm theo id (xóa bản ghi trong DB)
         * - Lưu ý: đây là xóa cứng; nếu muốn giữ lịch sử, hãy chuyển sang soft-delete
         * ========================= */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var p = await _context.Products!.FindAsync(id);
            if (p == null) return NotFound();

            // Xóa bản ghi sản phẩm
            _context.Products.Remove(p);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa sản phẩm thành công";
            return RedirectToAction(nameof(Products));
        }

        /* =========================
         * Users management
         * - Danh sách user, tìm kiếm theo username
         * ========================= */
        public IActionResult Users(string q)
        {
            var list = _context.Users?
                        .Where(u => string.IsNullOrEmpty(q) || (u.Username != null && u.Username.Contains(q)))
                        .OrderBy(u => u.Username)
                        .ToList()
                       ?? new List<User>();

            ViewBag.Query = q;

            // Trả view ở Views/Admin/Users/Index.cshtml
            return View("Users/Index", list);
        }

        /* =========================
         * ChangeRole (POST)
         * - Cập nhật quyền cho user (User <-> Admin)
         * ========================= */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(int id, string role)
        {
            var user = await _context.Users!.FindAsync(id);
            if (user == null) return NotFound();

            user.Role = role;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật quyền thành công";
            return RedirectToAction(nameof(Users));
        }

        /* =========================
         * DeleteUser (POST)
         * - Xóa tài khoản người dùng
         * - Lưu ý: cân nhắc soft-delete nếu cần bảo tồn dữ liệu
         * ========================= */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var u = await _context.Users!.FindAsync(id);
            if (u == null) return NotFound();

            _context.Users.Remove(u);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa tài khoản thành công";
            return RedirectToAction(nameof(Users));
        }
    }
}
