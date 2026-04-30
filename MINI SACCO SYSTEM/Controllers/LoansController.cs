using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MINI_SACCO_SYSTEM.Data;
using MINI_SACCO_SYSTEM.Models;

namespace MINI_SACCO_SYSTEM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LoansController : Controller
    {
        private readonly AppDbContext _context;

        public LoansController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var loans = await _context.Loans
                .Include(l => l.Member)
                .OrderByDescending(l => l.DateApplied)
                .ToListAsync();
            return View(loans);
        }

        public IActionResult Apply()
        {
            ViewBag.Members = new SelectList(_context.Members, "Id", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(Loans loan)
        {
            ModelState.Remove("Member");
            ModelState.Remove("Status");
            ModelState.Remove("DateApplied");

            if (ModelState.IsValid)
            {
                loan.Status = "Active";
                loan.DateApplied = DateTime.Now;
                loan.AmountRepaid = 0;
                _context.Loans.Add(loan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            TempData["Errors"] = string.Join(" | ", errors);

            ViewBag.Members = new SelectList(_context.Members, "Id", "FullName", loan.MemberId);
            return View(loan);
        }
    }
}