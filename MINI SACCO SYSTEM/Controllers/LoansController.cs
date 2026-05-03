using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MINI_SACCO_SYSTEM.Data;
using MINI_SACCO_SYSTEM.Models;
using MINI_SACCO_SYSTEM.Services;

namespace MINI_SACCO_SYSTEM.Controllers
{
    [Authorize]
    public class LoansController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly NotificationService _notificationService;

        public LoansController(AppDbContext context, UserManager<IdentityUser> userManager, NotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                var all = await _context.Loans
                    .Include(l => l.Member)
                    .OrderByDescending(l => l.DateApplied)
                    .ToListAsync();

                return View(all);
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);

                if (member == null)
                    return RedirectToAction("Index", "MemberPortal");

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

                if (member == null)
                    return RedirectToAction("Index", "MemberPortal");

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

                if (member == null)
                    return RedirectToAction("Index", "MemberPortal");

                loan.MemberId = member.Id;
            }

            if (loan.LoanPeriodMonths <= 0)
            {
                ModelState.AddModelError("LoanPeriodMonths", "Loan period must be greater than 0.");

                if (User.IsInRole("Admin"))
                {
                    ViewBag.Members = new SelectList(_context.Members, "Id", "FullName", loan.MemberId);
                    ViewBag.IsAdmin = true;
                }
                else
                {
                    ViewBag.IsAdmin = false;
                }

                return View(loan);
            }

            loan.InterestAmount = loan.Amount * (loan.InterestRate / 100) * loan.LoanPeriodMonths;
            loan.TotalPayable = loan.Amount + loan.InterestAmount;
            loan.MonthlyPayment = loan.TotalPayable / loan.LoanPeriodMonths;

            loan.Status = User.IsInRole("Admin") ? "Active" : "Pending";
            loan.DateApplied = DateTime.Now;

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            var memberApplied = await _context.Members.FindAsync(loan.MemberId);

            if (memberApplied != null)
            {
                await _notificationService.NotifyAdmin(
                    $"{memberApplied.FullName} applied for a loan of UGX {loan.Amount:N0}",
                    "/Loans"
                );

                if (memberApplied.UserId != null)
                {
                    await _notificationService.NotifyMember(
                        memberApplied.UserId,
                        $"Your loan application of UGX {loan.Amount:N0} has been submitted and is pending approval.",
                        "/Loans"
                    );
                }
            }

            if (User.IsInRole("Admin"))
                return RedirectToAction(nameof(Index));

            return RedirectToAction("Index", "MemberPortal");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var loan = await _context.Loans
                .Include(l => l.Member)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null)
                return NotFound();

            loan.Status = status;
            await _context.SaveChangesAsync();

            if (loan.Member?.UserId != null)
            {
                var msg = status == "Active"
                    ? $"🎉 Your loan of UGX {loan.Amount:N0} has been approved!"
                    : $"Your loan application of UGX {loan.Amount:N0} was rejected.";

                await _notificationService.NotifyMember(loan.Member.UserId, msg, "/Loans");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}