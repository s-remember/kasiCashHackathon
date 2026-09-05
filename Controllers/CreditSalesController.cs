using KasiCash.Data;
using KasiCash.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KasiCash.Controllers
{
    [Authorize]
    public class CreditSalesController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser>
            _userManager;

        public CreditSalesController(
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

            var records =
                await _context.CreditSales
                    .Include(c => c.Sale)
                    .ThenInclude(s =>
                        s!.Product)
                    .Where(c =>
                        c.Sale != null &&
                        c.Sale.OwnerId ==
                        userId)
                    .OrderBy(c => c.IsPaid)
                    .ThenBy(c =>
                        c.DueDate)
                    .ToListAsync();

            return View(records);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPaid(
            int id)
        {
            var userId =
                _userManager.GetUserId(User);

            var credit =
                await _context.CreditSales
                    .Include(c => c.Sale)
                    .FirstOrDefaultAsync(c =>
                        c.Id == id &&
                        c.Sale != null &&
                        c.Sale.OwnerId ==
                        userId);

            if (credit == null)
            {
                return NotFound();
            }

            credit.IsPaid = true;
            credit.PaidDate =
                DateTime.Now;

            await _context
                .SaveChangesAsync();

            TempData["Success"] =
                $"{credit.CustomerName}'s account has been marked paid.";

            return RedirectToAction(
                nameof(Index));
        }
    }
}