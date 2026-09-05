using Microsoft.AspNetCore.Mvc;

namespace KasiCash.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}