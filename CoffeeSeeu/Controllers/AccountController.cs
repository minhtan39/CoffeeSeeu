using Microsoft.AspNetCore.Mvc;
using CoffeeSeeu.Models;

namespace CoffeeSeeu.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login() => View();
        public IActionResult Register() => View();
    }
}
