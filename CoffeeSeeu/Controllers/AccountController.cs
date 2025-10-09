using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CoffeeSeeu.Models;

namespace CoffeeSeeu.Controllers
{
    public class AccountController : Controller
    {
        // Fake database tạm thời
        private static List<User> Users = new List<User>
        {
            new User { Id = 1, Username = "admin", Password = "123", Role = "Admin" },
            new User { Id = 2, Username = "user", Password = "123", Role = "Customer" }
        };

        public IActionResult Login() => View();
        public IActionResult Register() => View();
        public IActionResult AccessDenied() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = Users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user == null)
            {
                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu!";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Register(string username, string password)
        {
            if (Users.Any(u => u.Username == username))
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại!";
                return View();
            }

            Users.Add(new User
            {
                Id = Users.Count + 1,
                Username = username,
                Password = password,
                Role = "Customer"
            });

            ViewBag.Message = "Đăng ký thành công! Hãy đăng nhập.";
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
