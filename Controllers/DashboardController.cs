using KasiCash.Data;
using KasiCash.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KasiCash.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser>
            _userManager;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId =
                _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var user =
                await _userManager.GetUserAsync(User);

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var sales =
                await _context.Sales
                    .Where(s =>
                        s.OwnerId == userId)
                    .ToListAsync();

            var todaySales = sales
                .Where(s =>
                    s.SaleDate >= today &&
                    s.SaleDate < tomorrow)
                .ToList();

            var products =
                await _context.Products
                    .Where(p =>
                        p.OwnerId == userId)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

            var creditRecords =
                await _context.CreditSales
                    .Include(c => c.Sale)
                    .Where(c =>
                        c.Sale != null &&
                        c.Sale.OwnerId == userId)
                    .ToListAsync();

            var recentSales =
                await _context.Sales
                    .Include(s => s.Product)
                    .Where(s =>
                        s.OwnerId == userId)
                    .OrderByDescending(
                        s => s.SaleDate)
                    .Take(6)
                    .ToListAsync();

            var lowStock =
                products
                    .Where(p =>
                        p.QuantityInStock <=
                        p.LowStockLevel)
                    .ToList();

            var outstanding =
                creditRecords
                    .Where(c => !c.IsPaid)
                    .ToList();

            var model =
                new DashboardViewModel
                {
                    BusinessName =
                        user?.BusinessName ??
                        "My Business",

                    TodaySales =
                        todaySales.Sum(
                            s => s.TotalAmount),

                    TodayTransactions =
                        todaySales.Count,

                    OutstandingCredit =
                        outstanding.Sum(
                            c => c.AmountOwed),

                    OutstandingCustomers =
                        outstanding
                            .Select(c =>
                                c.CustomerName)
                            .Distinct()
                            .Count(),

                    TotalStockItems =
                        products.Sum(
                            p =>
                                p.QuantityInStock),

                    ProductCount =
                        products.Count,

                    LowStockItems =
                        lowStock.Count,

                    TotalSales =
                        sales.Sum(
                            s => s.TotalAmount),

                    CashSales =
                        sales
                            .Where(s =>
                                s.PaymentType ==
                                "Cash")
                            .Sum(s =>
                                s.TotalAmount),

                    CreditSales =
                        sales
                            .Where(s =>
                                s.PaymentType ==
                                "Credit")
                            .Sum(s =>
                                s.TotalAmount),

                    RecentSales =
                        recentSales,

                    LowStockProducts =
                        lowStock
                };

            return View(model);
        }
    }
}