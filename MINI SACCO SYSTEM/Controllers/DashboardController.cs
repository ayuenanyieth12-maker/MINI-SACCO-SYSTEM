using Microsoft.AspNetCore.Mvc;

namespace MINI_SACCO_SYSTEM.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
