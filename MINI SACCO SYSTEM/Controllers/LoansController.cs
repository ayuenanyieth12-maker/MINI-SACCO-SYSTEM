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
                    .Include(l => l.Repayments)
                    .OrderByDescending(l => l.DateApplied)
                    .ToListAsync();
                return View(all);
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
                if (member == null) return RedirectToAction("Index", "MemberPortal");

                var mine = await _context.Loans
                    .Include(l => l.Member)
                    .Include(l => l.Repayments)
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
            ModelState.Remove("Repayments");

            if (!User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User);
                var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
                if (member == null) return RedirectToAction("Index", "MemberPortal");
                loan.MemberId = member.Id;
            }

            if (ModelState.IsValid)
            {
                loan.Status = User.IsInRole("Admin") ? "Active" : "Pending";
                loan.DateApplied = DateTime.Now;
                loan.AmountRepaid = 0;
                _context.Loans.Add(loan);
                await _context.SaveChangesAsync();

                var member = await _context.Members.FindAsync(loan.MemberId);
                if (member != null)
                {
                    await _notificationService.NotifyAdmin(
                        $"{member.FullName} applied for a loan of UGX {loan.Amount:N0}", "/Loans");
                    if (member.UserId != null)
                        await _notificationService.NotifyMember(member.UserId,
                            $"Your loan application of UGX {loan.Amount:N0} is pending approval.", "/Loans");
                }

                return RedirectToAction(User.IsInRole("Admin") ? nameof(Index) : "Index", "MemberPortal");
            }

            if (User.IsInRole("Admin"))
                ViewBag.Members = new SelectList(_context.Members, "Id", "FullName", loan.MemberId);
            return View(loan);
        }

        // Loan detail with repayment history
        public async Task<IActionResult> Detail(int id)
        {
            var loan = await _context.Loans
                .Include(l => l.Member)
                .Include(l => l.Repayments)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null) return NotFound();

            // Members can only see their own loans
            if (!User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User);
                var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
                if (member == null || loan.MemberId != member.Id)
                    return RedirectToAction("Index");
            }

            return View(loan);
        }

        // Record a repayment — members and admin can do this
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Repay(int loanId, decimal amount, string notes)
        {
            var loan = await _context.Loans
                .Include(l => l.Member)
                .FirstOrDefaultAsync(l => l.Id == loanId);

            if (loan == null) return NotFound();

            // Members can only repay their own loans
            if (!User.IsInRole("Admin"))
            {
                var userId = _userManager.GetUserId(User);
                var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
                if (member == null || loan.MemberId != member.Id)
                    return RedirectToAction("Index");
            }

            // Add repayment record
            var repayment = new LoanRepayment
            {
                LoanId = loanId,
                Amount = amount,
                PaidOn = DateTime.Now,
                Notes = notes
            };
            _context.LoanRepayments.Add(repayment);

            // Update amount repaid
            loan.AmountRepaid += amount;

            // Auto-mark as cleared if fully paid
            var totalPayable = loan.TotalPayable > 0 ? loan.TotalPayable : loan.Amount;
            if (loan.AmountRepaid >= totalPayable)
            {
                loan.Status = "Cleared";
                if (loan.Member?.UserId != null)
                    await _notificationService.NotifyMember(loan.Member.UserId,
                        $"Congratulations! Your loan of UGX {loan.Amount:N0} has been fully repaid.", "/Loans");
            }

            await _context.SaveChangesAsync();

            // Notify admin
            await _notificationService.NotifyAdmin(
                $"{loan.Member?.FullName} made a loan repayment of UGX {amount:N0}", "/Loans");

            return RedirectToAction("Detail", new { id = loanId });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var loan = await _context.Loans
                .Include(l => l.Member)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (loan == null) return NotFound();

            loan.Status = status;
            await _context.SaveChangesAsync();

            if (loan.Member?.UserId != null)
            {
                var msg = status == "Active"
                    ? $"Your loan of UGX {loan.Amount:N0} has been approved!"
                    : $"Your loan application of UGX {loan.Amount:N0} was rejected.";
                await _notificationService.NotifyMember(loan.Member.UserId, msg, "/Loans");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}