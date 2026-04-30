using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MINI_SACCO_SYSTEM.Data;

namespace MINI_SACCO_SYSTEM.Controllers
{
    [Authorize]
    public class MemberPortalController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public MemberPortalController(AppDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var member = await _db.Members
                .Include(m => m.SavingsTransactions)
                .Include(m => m.Loans)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (member == null)
                return View("NoProfile"); // shown if member has no linked account yet

            return View(member);
        }
    }
}