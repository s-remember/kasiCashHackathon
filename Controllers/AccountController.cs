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

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

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
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(businessName) ||
                string.IsNullOrWhiteSpace(businessType) ||
                string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error =
                    "Please complete all required fields.";

                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error =
                    "Passwords do not match.";

                return View();
            }

            var existingUser =
                await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                ViewBag.Error =
                    "An account with this email already exists.";

                return View();
            }

            var user = new ApplicationUser
            {
                UserName = email.Trim(),
                Email = email.Trim(),
                FullName = fullName.Trim(),
                BusinessName = businessName.Trim(),
                BusinessType = businessType.Trim(),
                PhoneNumber = phoneNumber?.Trim()
            };

            var result =
                await _userManager.CreateAsync(
                    user,
                    password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(
                    user,
                    isPersistent: false);

                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            ViewBag.Error = string.Join(
                " ",
                result.Errors.Select(
                    error => error.Description));

            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(
                    "Index",
                    "Dashboard");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email,
            string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error =
                    "Enter your email and password.";

                return View();
            }

            var result =
                await _signInManager.PasswordSignInAsync(
                    email.Trim(),
                    password,
                    isPersistent: false,
                    lockoutOnFailure: true);

            if (result.Succeeded)
            {
                TempData["Success"] =
                    "Account created successfully. Please sign in.";

                return RedirectToAction("Login");
            }

            if (result.IsLockedOut)
            {
                ViewBag.Error =
                    "Too many failed attempts. Please try again shortly.";

                return View();
            }

            ViewBag.Error =
                "Invalid email or password.";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                "Index",
                "Home");
        }
    }
}