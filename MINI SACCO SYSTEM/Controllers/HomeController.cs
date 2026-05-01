using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MINI_SACCO_SYSTEM.Data;
using MINI_SACCO_SYSTEM.Models;
using System.Diagnostics;

namespace MINI_SACCO_SYSTEM.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // If already logged in, redirect to the right place
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Index", "Dashboard");
                else
                    return RedirectToAction("Index", "MemberPortal");
            }

            // Public stats
            var totalMembers = await _context.Members.CountAsync();
            var totalSavings = await _context.SavingsTransactions
                .Where(s => s.Type == "Deposit")
                .SumAsync(s => (decimal?)s.Amount) ?? 0;
            var totalLoans = await _context.Loans.CountAsync();
            var totalDisbursed = await _context.Loans
                .Where(l => l.Status == "Approved")
                .SumAsync(l => (decimal?)l.Amount) ?? 0;

            ViewBag.TotalMembers = totalMembers;
            ViewBag.TotalSavings = totalSavings;
            ViewBag.TotalLoans = totalLoans;
            ViewBag.TotalDisbursed = totalDisbursed;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}