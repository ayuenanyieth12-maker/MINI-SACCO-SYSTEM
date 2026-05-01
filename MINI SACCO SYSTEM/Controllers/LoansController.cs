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
    public class LoansController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public LoansController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                // Admin sees all loans
                var all = await _context.Loans
                    .Include(l => l.Member)
                    .OrderByDescending(l => l.DateApplied)
                    .ToListAsync();
                return View(all);
            }
            else
            {
                // Member sees only their loans
                var userId = _userManager.GetUserId(User);
                var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
                if (member == null) return RedirectToAction("Index", "MemberPortal");

                var mine = await _context.Loans
                    .Include(l => l.Member)
                    .Where(l => l.MemberId == member.Id)
                    .OrderByDescending(l => l.DateApplied)
                    .ToListAsync();
                return View(mine);
            }
        }

        public async Task<IActionResult> Apply()
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
        public async Task<IActionResult> Apply(Loans loan)
        {
            ModelState.Remove("Member");
            ModelState.Remove("Status");
            ModelState.Remove("DateApplied");

            if (!User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User);
                var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
                if (member == null) return RedirectToAction("Index", "MemberPortal");
                loan.MemberId = member.Id;
            }

            if (ModelState.IsValid)
            {
                // Member applications go as "Pending" — admin approves
                // Admin applications go straight to "Active"
                loan.Status = User.IsInRole("Admin") ? "Active" : "Pending";
                loan.DateApplied = DateTime.Now;
                loan.AmountRepaid = 0;
                _context.Loans.Add(loan);
                await _context.SaveChangesAsync();

                if (User.IsInRole("Admin"))
                    return RedirectToAction(nameof(Index));
                else
                    return RedirectToAction("Index", "MemberPortal");
            }

            if (User.IsInRole("Admin"))
                ViewBag.Members = new SelectList(_context.Members, "Id", "FullName", loan.MemberId);

            return View(loan);
        }

        // Admin only — approve or reject a loan
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan == null) return NotFound();
            loan.Status = status;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}