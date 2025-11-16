using CoffeeSeeu.Data;
using CoffeeSeeu.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace CoffeeSeeu.Controllers
{
    /* =========================================================================
     * Mục đích   : Xử lý đăng ký, đăng nhập, đăng xuất và quản lý thông tin
     *              tài khoản người dùng (Profile, EditProfile).
     * ========================================================================= */

    public class AccountController : Controller
    {
        // DbContext để thao tác với database (Users ...)
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor: nhận ApplicationDbContext qua Dependency Injection
        /// </summary>
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // -------------------- REGISTER (GET) --------------------

        /// <summary>
        /// Mục đích: Trả về trang đăng ký (form)
        /// Tham số: (không)
        /// Trả về: View đăng ký
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            // Trả về view đăng ký tài khoản
            return View();
        }

        // -------------------- REGISTER (POST) --------------------

        /// <summary>
        /// Mục đích: Xử lý đăng ký tài khoản mới
        /// Tham số:
        ///   - user: model User chứa thông tin nhập từ form
        /// Trả về:
        ///   - Nếu hợp lệ: chuyển hướng đến Login
        ///   - Nếu lỗi: trả về View cùng model để hiển thị lỗi
        /// Ghi chú: Mã hóa mật khẩu trước khi lưu
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            // Kiểm tra dữ liệu nhập có hợp lệ không (DataAnnotations)
            if (!ModelState.IsValid)
                return View(user);

            // Kiểm tra trùng tên đăng nhập (username phải là duy nhất)
            var exist = _context.Users.FirstOrDefault(u => u.Username == user.Username);
            if (exist != null)
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại!";
                return View(user);
            }

            // Mặc định vai trò khi đăng ký là 'User'
            user.Role = "User";

            // Mã hóa mật khẩu trước khi lưu vào DB (dùng PasswordHasher của ASP.NET)
            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, user.Password ?? "");

            // Lưu user mới vào database
            _context.Users.Add(user);
            _context.SaveChanges();

            // Thông báo thành công (sử dụng TempData để hiển thị ở trang Login)
            TempData["Success"] = "Đăng ký thành công. Mời bạn đăng nhập.";
            return RedirectToAction("Login");
        }

        // -------------------- LOGIN (GET) --------------------

        /// <summary>
        /// Mục đích: Trả về trang đăng nhập
        /// Tham số: (không)
        /// Trả về: View đăng nhập hoặc chuyển hướng nếu đã đăng nhập
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập rồi thì chuyển hướng theo role
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

        /// <summary>
        /// Mục đích: Xử lý đăng nhập bằng username/password
        /// Tham số:
        ///   - username: tên đăng nhập
        ///   - password: mật khẩu gốc (chưa mã hóa)
        ///   - remember: nếu true sẽ giữ login lâu dài (cookie persistent)
        /// Trả về:
        ///   - Nếu thành công: chuyển hướng theo role (Admin->Admin, khác->Home)
        ///   - Nếu thất bại: trả về View kèm thông báo lỗi
        /// </summary>
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

            // Đăng xuất người dùng cũ (nếu có) và xóa session cũ
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            // Tìm người dùng theo username
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu!";
                return View();
            }

            // Kiểm tra mật khẩu: so sánh mật khẩu input với mật khẩu đã hash trong DB
            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.Password ?? "", password ?? "");
            if (result == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu!";
                return View();
            }

            // -------------------- Tạo Claims (thông tin người dùng) --------------------
            // Tạo danh sách claim để lưu trong cookie (Name, Role, AvatarUrl)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? "User"),
                new Claim("AvatarUrl", user.AvatarUrl ?? "/img/default-avatar.png")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Đăng nhập với Cookie (tùy chọn IsPersistent nếu remember = true)
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = remember }
            );

            // Lưu một số thông tin vào Session để dùng tiện lợi ở View/layout
            HttpContext.Session.SetString("Username", user.Username ?? "");
            HttpContext.Session.SetString("Role", user.Role ?? "User");

            // Chuyển hướng sau khi đăng nhập theo role
            return user.Role == "Admin"
                ? RedirectToAction("Index", "Admin")
                : RedirectToAction("Index", "Home");
        }

        // -------------------- LOGOUT --------------------

        /// <summary>
        /// Mục đích: Đăng xuất người dùng, xóa cookie và session
        /// Tham số: (không)
        /// Trả về: Chuyển hướng về trang Login
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Sign out cookie authentication và clear session
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // -------------------- ACCESS DENIED --------------------

        /// <summary>
        /// Mục đích: Trang hiển thị khi truy cập bị từ chối (Authorize)
        /// Tham số: (không)
        /// Trả về: View AccessDenied
        /// </summary>
        public IActionResult AccessDenied()
        {
            // Trang hiển thị khi truy cập bị từ chối
            return View();
        }

        // =========================================================
        // ============= CÁC CHỨC NĂNG THÔNG TIN CÁ NHÂN =============
        // =========================================================

        // --- Hiển thị thông tin cá nhân ---

        /// <summary>
        /// Mục đích: Hiển thị trang Profile của user đang đăng nhập
        /// Tham số: (không)
        /// Trả về: View(Profile) với model User
        /// </summary>
        [HttpGet]
        public IActionResult Profile()
        {
            // Lấy username từ claim (ClaimTypes.Name)
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login");

            // Lấy thông tin user từ DB theo username
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return NotFound();

            // Trả về view cùng model User (để hiển thị thông tin)
            return View(user);
        }

        // --- Chỉnh sửa tài khoản (GET) ---

        /// <summary>
        /// Mục đích: Trả về form chỉnh sửa profile cho user hiện tại
        /// Tham số: (không)
        /// Trả về: View(EditProfile) với model User
        /// </summary>
        [HttpGet]
        public IActionResult EditProfile()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return NotFound();

            // Hiển thị form chỉnh sửa (đã có sẵn thông tin)
            return View(user);
        }

        // --- Chỉnh sửa tài khoản (POST) ---

        /// <summary>
        /// Mục đích: Xử lý cập nhật thông tin user và upload avatar (nếu có)
        /// Tham số:
        ///   - model: User chứa thông tin mới (Email, Phone, FullName)
        ///   - AvatarFile: file ảnh upload (tuỳ chọn)
        /// Trả về:
        ///   - Redirect đến Profile khi thành công
        ///   - NotFound nếu user không tồn tại
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(User model, IFormFile? AvatarFile)
        {
            // Lấy user hiện tại theo username từ claim
            var username = User.Identity?.Name;
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return NotFound();

            // Cập nhật các trường cơ bản
            user.Email = model.Email;
            user.Phone = model.Phone;
            user.FullName = model.FullName;

            // Nếu có upload ảnh đại diện mới -> lưu file vào wwwroot và cập nhật đường dẫn
            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                // Tạo tên file duy nhất tránh trùng
                var fileName = $"{Guid.NewGuid()}_{AvatarFile.FileName}";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/avatars", fileName);

                // Tạo thư mục nếu chưa tồn tại
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                // Ghi file lên server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await AvatarFile.CopyToAsync(stream);
                }

                // Cập nhật đường dẫn ảnh trong user
                user.AvatarUrl = $"/img/avatars/{fileName}";
            }

            // Lưu thay đổi vào database
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Thông báo thành công và chuyển về trang Profile
            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Profile");
        }
    }
}
