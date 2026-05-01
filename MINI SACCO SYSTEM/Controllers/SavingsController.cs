using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MINI_SACCO_SYSTEM.Data;
using MINI_SACCO_SYSTEM.Models;

namespace MINI_SACCO_SYSTEM.Controllers
{
    [Authorize]
    public class SavingsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SavingsController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                // Admin sees all transactions
                var all = await _context.SavingsTransactions
                    .Include(t => t.Member)
                    .OrderByDescending(t => t.Date)
                    .ToListAsync();
                return View(all);
            }
            else
            {
                // Member sees only their own
                var userId = _userManager.GetUserId(User);
                var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
                if (member == null) return RedirectToAction("Index", "MemberPortal");

                var mine = await _context.SavingsTransactions
                    .Include(t => t.Member)
                    .Where(t => t.MemberId == member.Id)
                    .OrderByDescending(t => t.Date)
                    .ToListAsync();
                return View(mine);
            }
        }

        public async Task<IActionResult> Deposit()
        {
            if (User.IsInRole("Admin"))
            {
                ViewBag.Members = new SelectList(_context.Members, "Id", "FullName");
                ViewBag.IsAdmin = true;
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
                if (member == null) return RedirectToAction("Index", "MemberPortal");
                ViewBag.MemberId = member.Id;
                ViewBag.IsAdmin = false;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(Savings transaction)
        {
            ModelState.Remove("Member");
            ModelState.Remove("Type");

            if (!User.IsInRole("Admin"))
            {
                // Force member id for non-admins
                var userId = _userManager.GetUserId(User);
                var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
                if (member == null) return RedirectToAction("Index", "MemberPortal");
                transaction.MemberId = member.Id;
            }

            if (ModelState.IsValid)
            {
                transaction.Type = "Deposit";
                transaction.Date = DateTime.Now;
                _context.SavingsTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                if (User.IsInRole("Admin"))
                    return RedirectToAction(nameof(Index));
                else
                    return RedirectToAction("Index", "MemberPortal");
            }

            if (User.IsInRole("Admin"))
                ViewBag.Members = new SelectList(_context.Members, "Id", "FullName", transaction.MemberId);

            return View(transaction);
        }

        public async Task<IActionResult> Withdraw()
        {
            if (User.IsInRole("Admin"))
            {
                ViewBag.Members = new SelectList(_context.Members, "Id", "FullName");
                ViewBag.IsAdmin = true;
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
                if (member == null) return RedirectToAction("Index", "MemberPortal");
                ViewBag.MemberId = member.Id;
                ViewBag.IsAdmin = false;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(Savings transaction)
        {
            ModelState.Remove("Member");
            ModelState.Remove("Type");

            if (!User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User);
                var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
                if (member == null) return RedirectToAction("Index", "MemberPortal");
                transaction.MemberId = member.Id;
            }

            if (ModelState.IsValid)
            {
                transaction.Type = "Withdrawal";
                transaction.Date = DateTime.Now;
                _context.SavingsTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                if (User.IsInRole("Admin"))
                    return RedirectToAction(nameof(Index));
                else
                    return RedirectToAction("Index", "MemberPortal");
            }

            if (User.IsInRole("Admin"))
                ViewBag.Members = new SelectList(_context.Members, "Id", "FullName", transaction.MemberId);

            return View(transaction);
        }
    }
}