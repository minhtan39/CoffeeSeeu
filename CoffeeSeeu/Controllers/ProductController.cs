using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeSeeu.Data;
using CoffeeSeeu.Models;

namespace CoffeeSeeu.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Product?searchString=...
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["SearchQuery"] = searchString;

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var q = searchString.Trim();
                var qLower = q.ToLower(); // dùng ToLower để EF dịch ra SQL

                // 1) exact match (case-insensitive) - tránh null deref
                var exact = await _context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Name != null && p.Name.ToLower() == qLower);

                if (exact != null)
                {
                    // redirect to Details by id
                    return RedirectToAction(nameof(Details), new { id = exact.Id });
                }

                // 2) partial match on name or description (case-insensitive)
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

            // no search -> show all products (or you can limit)
            var all = await _context.Products
                .AsNoTracking()
                .OrderByDescending(p => p.Rating)
                .ToListAsync();

            return View(all);
        }

        // GET => Suggest (autocomplete)
        // GET /Product/Suggest?q=peach
        [HttpGet]
        public async Task<IActionResult> Suggest(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(new object[0]);

            var qLower = q.Trim().ToLower();

            var suggestions = await _context.Products
                .AsNoTracking()
                .Where(p => p.Name != null && EF.Functions.Like(p.Name.ToLower(), $"%{qLower}%"))
                .OrderByDescending(p => p.Rating)
                .Select(p => new { p.Id, p.Name })
                .Take(10)
                .ToListAsync();

            return Json(suggestions);
        }

        // GET: /Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id.Value);

            if (product == null) return NotFound();

            return View(product);
        }

        // ====== PHẦN ĐÁNH GIÁ (async) ======
        [HttpPost]
        public async Task<IActionResult> RateProduct([FromBody] RateRequest req)
        {
            if (req == null || req.ProductId <= 0)
                return BadRequest();

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == req.ProductId);
            if (product == null)
                return NotFound();

            req.Rating = System.Math.Clamp(req.Rating, 1, 5);
            product.Rating = req.Rating;
            await _context.SaveChangesAsync();

            return Json(new { success = true, rating = req.Rating });
        }
    }

    // Lớp nhận dữ liệu JSON từ fetch
    public class RateRequest
    {
        public int ProductId { get; set; }
        public int Rating { get; set; }
    }
}
