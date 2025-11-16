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
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Dashboard
        public IActionResult Index()
        {
            // defensive: _context.* có thể null trong một vài tình huống test, nên dùng null-conditional
            ViewBag.ProductCount = _context.Products?.Count() ?? 0;
            ViewBag.UserCount = _context.Users?.Count() ?? 0;
            ViewBag.OrderCount = _context.Orders?.Count() ?? 0;
            return View();
        }

        // ---------------- Products ----------------
        // GET: /Admin/Products
        public async Task<IActionResult> Products(string q, int page = 1, int pageSize = 20)
        {
            var query = _context.Products?.AsQueryable() ?? Enumerable.Empty<Product>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(p => p.Name != null && p.Name.Contains(q));
            }

            var items = await query
                .OrderByDescending(p => p.Id)
                .Skip((Math.Max(1, page) - 1) * Math.Max(1, pageSize))
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Query = q;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            // explicit path to Views/Admin/Products/Index.cshtml
            return View("Products/Index", items);
        }

        // GET: /Admin/CreateProduct
        public IActionResult CreateProduct()
        {
            return View("Products/Create", new Product());
            // or using target-typed new: return View("Products/Create", new());
        }

        // POST: /Admin/CreateProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product model, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
            {
                return View("Products/Create", model);
            }

            // handle image upload
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var ext = Path.GetExtension(ImageFile.FileName) ?? "";
                // use case-insensitive comparer
                var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("", "Chỉ chấp nhận file ảnh (jpg, png, webp).");
                    return View("Products/Create", model);
                }

                var fileName = $"{Guid.NewGuid()}{ext}";
                var saveFolder = Path.Combine(_env.WebRootPath, "img", "products");
                Directory.CreateDirectory(saveFolder);
                var savePath = Path.Combine(saveFolder, fileName);
                using (var fs = new FileStream(savePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fs);
                }

                model.ImageUrl = $"/img/products/{fileName}";
            }

            _context.Products.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Thêm sản phẩm thành công";
            return RedirectToAction(nameof(Products));
        }

        // GET: /Admin/EditProduct/5
        public async Task<IActionResult> EditProduct(int id)
        {
            var p = await _context.Products!.FindAsync(id);
            if (p == null) return NotFound();
            return View("Products/Edit", p);
        }

        // POST: /Admin/EditProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product model, IFormFile? ImageFile)
        {
            var p = await _context.Products!.FindAsync(id);
            if (p == null) return NotFound();

            if (!ModelState.IsValid) return View("Products/Edit", model);

            p.Name = model.Name;
            p.Description = model.Description;
            p.Price = model.Price;
            p.Rating = model.Rating;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var ext = Path.GetExtension(ImageFile.FileName) ?? "";
                var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("", "Chỉ chấp nhận file ảnh (jpg, png, webp).");
                    return View("Products/Edit", model);
                }

                var fileName = $"{Guid.NewGuid()}{ext}";
                var saveFolder = Path.Combine(_env.WebRootPath, "img", "products");
                Directory.CreateDirectory(saveFolder);
                var savePath = Path.Combine(saveFolder, fileName);
                using (var fs = new FileStream(savePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fs);
                }

                // try delete old image if exists and not default
                try
                {
                    if (!string.IsNullOrEmpty(p.ImageUrl) && !p.ImageUrl.Contains("default", StringComparison.OrdinalIgnoreCase))
                    {
                        var old = Path.Combine(_env.WebRootPath, p.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(old)) System.IO.File.Delete(old);
                    }
                }
                catch { /* ignore */ }

                p.ImageUrl = $"/img/products/{fileName}";
            }

            _context.Products.Update(p);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật sản phẩm thành công";
            return RedirectToAction(nameof(Products));
        }

        // POST: /Admin/DeleteProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var p = await _context.Products!.FindAsync(id);
            if (p == null) return NotFound();

            // Hard delete (you can change to soft delete if you add a flag)
            _context.Products.Remove(p);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa sản phẩm thành công";
            return RedirectToAction(nameof(Products));
        }

        // ---------------- Users management ----------------
        // GET: /Admin/Users
        public IActionResult Users(string q)
        {
            var list = _context.Users?
                        .Where(u => string.IsNullOrEmpty(q) || (u.Username != null && u.Username.Contains(q)))
                        .OrderBy(u => u.Username)
                        .ToList()
                       ?? new List<User>();
            ViewBag.Query = q;

            // explicit path to Views/Admin/Users/Index.cshtml
            return View("Users/Index", list);
        }

        // POST: /Admin/ChangeRole
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

        // POST: /Admin/DeleteUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var u = await _context.Users!.FindAsync(id);
            if (u == null) return NotFound();

            _context.Users.Remove(u); // consider soft-delete in future
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa tài khoản thành công";
            return RedirectToAction(nameof(Users));
        }
    }
}
