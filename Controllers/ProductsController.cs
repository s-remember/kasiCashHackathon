using KasiCash.Data;
using KasiCash.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KasiCash.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================
        // VIEW STOCK
        // =========================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var products = await _context.Products
                .Where(p => p.OwnerId == userId)
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(products);
        }

        // =========================
        // ADD PRODUCT
        // =========================

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Product
            {
                LowStockLevel = 5
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            // OwnerId is assigned by the server,
            // not entered by the shop owner.
            ModelState.Remove(nameof(Product.OwnerId));
            ModelState.Remove(nameof(Product.Owner));

            if (string.IsNullOrWhiteSpace(product.Name))
            {
                ModelState.AddModelError(
                    nameof(Product.Name),
                    "Product name is required.");
            }

            if (product.SellingPrice <= 0)
            {
                ModelState.AddModelError(
                    nameof(Product.SellingPrice),
                    "Selling price must be greater than R0.");
            }

            if (product.QuantityInStock < 0)
            {
                ModelState.AddModelError(
                    nameof(Product.QuantityInStock),
                    "Stock cannot be negative.");
            }

            if (product.LowStockLevel < 0)
            {
                ModelState.AddModelError(
                    nameof(Product.LowStockLevel),
                    "Low stock level cannot be negative.");
            }

            if (!ModelState.IsValid)
            {
                return View(product);
            }

            product.Name = product.Name.Trim();
            product.OwnerId = userId;

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"{product.Name} was added successfully.";

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT PRODUCT / RESTOCK
        // =========================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.OwnerId == userId);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Product input)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.OwnerId == userId);

            if (product == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Product.OwnerId));
            ModelState.Remove(nameof(Product.Owner));

            if (!ModelState.IsValid)
            {
                input.Id = id;
                input.OwnerId = userId;

                return View(input);
            }

            product.Name = input.Name.Trim();
            product.SellingPrice =
                input.SellingPrice;

            product.QuantityInStock =
                input.QuantityInStock;

            product.LowStockLevel =
                input.LowStockLevel;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"{product.Name} was updated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}