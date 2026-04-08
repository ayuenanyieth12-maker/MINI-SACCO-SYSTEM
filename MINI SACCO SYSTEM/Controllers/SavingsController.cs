using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MINI_SACCO_SYSTEM.Data;
using MINI_SACCO_SYSTEM.Models;

namespace MINI_SACCO_SYSTEM.Controllers
{
    public class SavingsController : Controller
    {
        private readonly AppDbContext _context;

        public SavingsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var transactions = await _context.SavingsTransactions
                .Include(t => t.Member)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            return View(transactions);
        }

        public IActionResult Deposit()
        {
            ViewBag.Members = new SelectList(_context.Members, "Id", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(Savings transaction)
        {
            ModelState.Remove("Member");
            ModelState.Remove("Type");

            if (ModelState.IsValid)
            {
                transaction.Type = "Deposit";
                transaction.Date = DateTime.Now;
                _context.SavingsTransactions.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Members = new SelectList(_context.Members, "Id", "FullName", transaction.MemberId);
            return View(transaction);
        }

        public IActionResult Withdraw()
        {
            ViewBag.Members = new SelectList(_context.Members, "Id", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(Savings transaction)
        {
            ModelState.Remove("Member");
            ModelState.Remove("Type");

            if (ModelState.IsValid)
            {
                transaction.Type = "Withdrawal";
                transaction.Date = DateTime.Now;
                _context.SavingsTransactions.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Members = new SelectList(_context.Members, "Id", "FullName", transaction.MemberId);
            return View(transaction);
        }
    }
}