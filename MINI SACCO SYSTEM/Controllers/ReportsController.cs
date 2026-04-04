using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MINI_SACCO_SYSTEM.Data;
using MINI_SACCO_SYSTEM.ViewModels;

namespace MINI_SACCO_SYSTEM.Controllers
{
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            var report = new ReportViewModel
            {
                TotalMembers = await _context.Members.CountAsync(),

                NewMembersThisMonth = await _context.Members
                    .Where(m => m.DateJoined >= startOfMonth)
                    .CountAsync(),

                TotalSavings = await _context.SavingsTransactions
                    .Where(t => t.Type == "Deposit")
                    .SumAsync(t => (decimal?)t.Amount) ?? 0
                    - (await _context.SavingsTransactions
                    .Where(t => t.Type == "Withdrawal")
                    .SumAsync(t => (decimal?)t.Amount) ?? 0),

                DepositsThisMonth = await _context.SavingsTransactions
                    .Where(t => t.Type == "Deposit" && t.Date >= startOfMonth)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0,

                WithdrawalsThisMonth = await _context.SavingsTransactions
                    .Where(t => t.Type == "Withdrawal" && t.Date >= startOfMonth)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0,

                TotalActiveLoans = await _context.Loans
                    .Where(l => l.Status == "Active")
                    .CountAsync(),

                OverdueLoans = await _context.Loans
                    .Where(l => l.Status == "Overdue")
                    .CountAsync(),

                ClearedLoans = await _context.Loans
                    .Where(l => l.Status == "Cleared")
                    .CountAsync(),

                TotalLoanBook = await _context.Loans
                    .Where(l => l.Status != "Cleared")
                    .SumAsync(l => (decimal?)l.Amount) ?? 0,

                TotalRepaid = await _context.Loans
                    .SumAsync(l => (decimal?)l.AmountRepaid) ?? 0
            };

            return View(report);
        }
    }
}