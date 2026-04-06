using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MINI_SACCO_SYSTEM.Data;
using MINI_SACCO_SYSTEM.Models;

namespace MINI_SACCO_SYSTEM.Controllers
{
    
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
            if (ModelState.IsValid)
            {
                loan.Status = "Active";
                loan.DateApplied = DateTime.Now;
                loan.AmountRepaid = 0;
                _context.Loans.Add(loan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Members = new SelectList(_context.Members, "Id", "FullName");
            return View(loan);
        }
    }
}