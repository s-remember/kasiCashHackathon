using KasiCash.Data;
using KasiCash.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KasiCash.Controllers
{
    [Authorize]
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser>
            _userManager;

        public SalesController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId =
                _userManager.GetUserId(User);

            var products =
                await _context.Products
                    .Where(p =>
                        p.OwnerId == userId &&
                        p.QuantityInStock > 0)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteSale(
            int productId,
            int quantity,
            string paymentType,
            string? customerName,
            string? customerPhone,
            DateTime? dueDate)
        {
            var userId =
                _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var product =
                await _context.Products
                    .FirstOrDefaultAsync(p =>
                        p.Id == productId &&
                        p.OwnerId == userId);

            if (product == null)
            {
                TempData["Error"] =
                    "Product not found.";

                return RedirectToAction(
                    nameof(Create));
            }

            if (quantity < 1)
            {
                TempData["Error"] =
                    "Quantity must be at least 1.";

                return RedirectToAction(
                    nameof(Create));
            }

            if (quantity >
                product.QuantityInStock)
            {
                TempData["Error"] =
                    $"Only {product.QuantityInStock} item(s) are available.";

                return RedirectToAction(
                    nameof(Create));
            }

            if (paymentType != "Cash" &&
                paymentType != "Credit")
            {
                TempData["Error"] =
                    "Choose Cash or Credit.";

                return RedirectToAction(
                    nameof(Create));
            }

            if (paymentType == "Credit" &&
                string.IsNullOrWhiteSpace(
                    customerName))
            {
                TempData["Error"] =
                    "Customer name is required for credit sales.";

                return RedirectToAction(
                    nameof(Create));
            }

            var total =
                product.SellingPrice *
                quantity;

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                var sale =
                    new Sale
                    {
                        ProductId =
                            product.Id,

                        Quantity =
                            quantity,

                        TotalAmount =
                            total,

                        PaymentType =
                            paymentType,

                        SaleDate =
                            DateTime.Now,

                        OwnerId =
                            userId
                    };

                product.QuantityInStock -=
                    quantity;

                _context.Sales.Add(sale);

                await _context
                    .SaveChangesAsync();

                if (paymentType == "Credit")
                {
                    var credit =
                        new CreditSale
                        {
                            SaleId =
                                sale.Id,

                            CustomerName =
                                customerName!
                                    .Trim(),

                            CustomerPhone =
                                string.IsNullOrWhiteSpace(
                                    customerPhone)
                                    ? null
                                    : customerPhone.Trim(),

                            AmountOwed =
                                total,

                            DueDate =
                                dueDate ??
                                DateTime.Today
                                    .AddDays(7),

                            IsPaid =
                                false
                        };

                    _context.CreditSales
                        .Add(credit);

                    await _context
                        .SaveChangesAsync();
                }

                await transaction
                    .CommitAsync();

                TempData["Success"] =
                    $"{product.Name} sale recorded — R{total:0.00}";
            }
            catch
            {
                await transaction
                    .RollbackAsync();

                TempData["Error"] =
                    "The sale could not be completed.";
            }

            return RedirectToAction(
                nameof(Create));
        }
    }
}