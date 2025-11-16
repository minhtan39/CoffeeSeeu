using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeSeeu.Data;
using CoffeeSeeu.Models;

namespace CoffeeSeeu.Controllers
{
    /* ============================================================================
     *  File        : ProductController.cs
     *  Mục đích    : Xử lý chức năng hiển thị sản phẩm, tìm kiếm, gợi ý và đánh giá
     * ============================================================================ */

    public class ProductController : Controller
    {
        // DbContext để truy cập bảng Products
        private readonly ApplicationDbContext _context;

        // Constructor nhận DbContext thông qua DI
        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        /* ============================================================================
         *  GET: /Product?searchString=...
         *  Chức năng:
         *      - Hiển thị danh sách tất cả sản phẩm
         *      - Nếu có search => tìm kiếm theo tên và mô tả
         *      - Nếu tên trùng khớp 100% => chuyển thẳng đến trang chi tiết sản phẩm
         * ============================================================================ */
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["SearchQuery"] = searchString;

            // Nếu có chuỗi tìm kiếm
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var q = searchString.Trim();
                var qLower = q.ToLower();

                // ---- 1. Tìm chính xác tên sản phẩm (không phân biệt hoa thường) ----
                var exact = await _context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p =>
                        p.Name != null &&
                        p.Name.ToLower() == qLower
                    );

                // Nếu tìm đúng tên thì chuyển hướng sang trang chi tiết
                if (exact != null)
                    return RedirectToAction(nameof(Details), new { id = exact.Id });

                // ---- 2. Tìm kiếm gần đúng theo tên hoặc mô tả (LIKE %key%) ----
                var partial = await _context.Products
                    .AsNoTracking()
                    .Where(p =>
                        (p.Name != null && EF.Functions.Like(p.Name.ToLower(), $"%{qLower}%")) ||
                        (p.Description != null && EF.Functions.Like(p.Description.ToLower(), $"%{qLower}%"))
                    )
                    .OrderByDescending(p => p.Rating)
                    .ToListAsync();

                return View(partial);
            }

            // ---- Không tìm kiếm => trả toàn bộ sản phẩm ----
            var all = await _context.Products
                .AsNoTracking()
                .OrderByDescending(p => p.Rating)
                .ToListAsync();

            return View(all);
        }

        /* ============================================================================
         *  GET: /Product/Suggest?q=...
         *  Chức năng:
         *      - Tạo danh sách gợi ý autocomplete cho thanh tìm kiếm
         *      - Trả về JSON gồm Id + Name
         * ============================================================================ */
        [HttpGet]
        public async Task<IActionResult> Suggest(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(new object[0]); // trả rỗng

            var qLower = q.Trim().ToLower();

            var suggestions = await _context.Products
                .AsNoTracking()
                .Where(p =>
                    p.Name != null &&
                    EF.Functions.Like(p.Name.ToLower(), $"%{qLower}%")
                )
                .OrderByDescending(p => p.Rating)
                .Select(p => new { p.Id, p.Name })
                .Take(10)
                .ToListAsync();

            return Json(suggestions);
        }

        /* ============================================================================
         *  GET: /Product/Details/5
         *  Chức năng:
         *      - Hiển thị chi tiết sản phẩm theo id
         * ============================================================================ */
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id.Value);

            if (product == null)
                return NotFound();

            return View(product);
        }

        /* ============================================================================
         *  POST: /Product/RateProduct
         *  Chức năng:
         *      - Nhận JSON từ client để cập nhật rating sản phẩm
         *      - Rating bị ép giá trị từ 1 → 5 để tránh lỗi nhập sai
         * ============================================================================ */
        [HttpPost]
        public async Task<IActionResult> RateProduct([FromBody] RateRequest req)
        {
            if (req == null || req.ProductId <= 0)
                return BadRequest();

            // Tìm sản phẩm theo id
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == req.ProductId);

            if (product == null)
                return NotFound();

            // Ép rating vào giới hạn 1–5
            req.Rating = System.Math.Clamp(req.Rating, 1, 5);

            product.Rating = req.Rating;
            await _context.SaveChangesAsync();

            return Json(new { success = true, rating = req.Rating });
        }
    }

    /* ============================================================================
     *  Lớp Model nhận dữ liệu JSON khi client gửi đánh giá sản phẩm
     * ============================================================================ */
    public class RateRequest
    {
        public int ProductId { get; set; }  // Id của sản phẩm được đánh giá
        public int Rating { get; set; }     // Số sao (1–5)
    }
}
