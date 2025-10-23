using CoffeeSeeu.Data;
using CoffeeSeeu.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace CoffeeSeeu.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor: nhận context để thao tác với database
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // -------------------- REGISTER (GET) --------------------
        [HttpGet]
        public IActionResult Register()
        {
            // Trả về view đăng ký tài khoản
            return View();
        }

        // -------------------- REGISTER (POST) --------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            // Kiểm tra dữ liệu nhập có hợp lệ không
            if (!ModelState.IsValid)
                return View(user);

            // Kiểm tra trùng tên đăng nhập
            var exist = _context.Users.FirstOrDefault(u => u.Username == user.Username);
            if (exist != null)
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại!";
                return View(user);
            }

            // Mặc định vai trò khi đăng ký là 'User'
            user.Role = "User";

            // Mã hóa mật khẩu
            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, user.Password ?? "");

            // Lưu vào database
            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["Success"] = "Đăng ký thành công. Mời bạn đăng nhập.";
            return RedirectToAction("Login");
        }

        // -------------------- LOGIN (GET) --------------------
        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập rồi thì chuyển hướng đúng vai trò
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Index", "Admin");
                else
                    return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // -------------------- LOGIN (POST) --------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, bool remember = false)
        {
            // Kiểm tra nhập thiếu
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
                return View();
            }

            // Đăng xuất người dùng cũ (nếu có)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            // Tìm người dùng theo username
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu!";
                return View();
            }

            // Kiểm tra mật khẩu đã mã hóa
            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.Password ?? "", password ?? "");
            if (result == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu!";
                return View();
            }

            // -------------------- Tạo Claims (thông tin người dùng) --------------------
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? "User"),
                new Claim("AvatarUrl", user.AvatarUrl ?? "/img/default-avatar.png")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Đăng nhập với Cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = remember }
            );

            // Lưu thông tin vào Session
            HttpContext.Session.SetString("Username", user.Username ?? "");
            HttpContext.Session.SetString("Role", user.Role ?? "User");

            // Chuyển hướng sau khi đăng nhập
            return user.Role == "Admin"
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Index", "Home");
        }

        // -------------------- LOGOUT --------------------
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Xóa cookie và session khi đăng xuất
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // -------------------- ACCESS DENIED --------------------
        public IActionResult AccessDenied()
        {
            // Trang hiển thị khi truy cập bị từ chối
            return View();
        }

        // =========================================================
        // ============= CÁC CHỨC NĂNG THÔNG TIN CÁ NHÂN ==========
        // =========================================================

        // --- Hiển thị thông tin cá nhân ---
        [HttpGet]
        public IActionResult Profile()
        {
            // Lấy username từ cookie đăng nhập
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login");

            // Lấy thông tin người dùng từ DB
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return NotFound();

            // Trả về view cùng model User
            return View(user);
        }

        // --- Chỉnh sửa tài khoản (GET) ---
        [HttpGet]
        public IActionResult EditProfile()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return NotFound();

            // Hiển thị form chỉnh sửa
            return View(user);
        }

        // --- Chỉnh sửa tài khoản (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(User model, IFormFile? AvatarFile)
        {
            // Lấy user hiện tại theo username trong cookie
            var username = User.Identity?.Name;
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return NotFound();

            // Cập nhật các thông tin cơ bản
            user.Email = model.Email;
            user.Phone = model.Phone;
            user.FullName = model.FullName;

            // Nếu có upload ảnh đại diện mới
            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                // Tạo tên file duy nhất
                var fileName = $"{Guid.NewGuid()}_{AvatarFile.FileName}";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/avatars", fileName);

                // Tạo thư mục nếu chưa có
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                // Ghi file ảnh vào server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await AvatarFile.CopyToAsync(stream);
                }

                // Cập nhật đường dẫn ảnh mới
                user.AvatarUrl = $"/img/avatars/{fileName}";
            }

            // Cập nhật thông tin trong DB
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Thông báo thành công
            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Profile");
        }
    }
}
