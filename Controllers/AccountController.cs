using KasiCash.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KasiCash.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // =========================
        // REGISTRATION
        // =========================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string fullName,
            string businessName,
            string businessType,
            string phoneNumber,
            string email,
            string password,
            string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                ViewBag.Error = "An account with this email already exists.";
                return View();
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                BusinessName = businessName,
                BusinessType = businessType,
                PhoneNumber = phoneNumber
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                return RedirectToAction("Login");
            }

            ViewBag.Error = string.Join(
                " ",
                result.Errors.Select(error => error.Description));

            return View();
        }

        // =========================
        // LOGIN
        // =========================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email,
            string password)
        {
            var result = await _signInManager.PasswordSignInAsync(
                email,
                password,
                isPersistent: false,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.Error = "Invalid email or password.";

            return View();
        }

        // =========================
        // LOGOUT
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Login");
        }
    }
}