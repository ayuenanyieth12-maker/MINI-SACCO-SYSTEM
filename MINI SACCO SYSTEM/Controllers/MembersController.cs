using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MINI_SACCO_SYSTEM.Data;
using MINI_SACCO_SYSTEM.Models;

namespace MINI_SACCO_SYSTEM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MembersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MembersController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var members = await _context.Members
                .OrderByDescending(m => m.DateJoined)
                .ToListAsync();
            return View(members);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Members member)
        {
            ModelState.Remove("Loans");
            ModelState.Remove("SavingsTransactions");
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                member.DateJoined = DateTime.Now;
                _context.Members.Add(member);
                await _context.SaveChangesAsync();

                // Create a login account for this member
                if (!string.IsNullOrEmpty(member.Email))
                {
                    var existingUser = await _userManager.FindByEmailAsync(member.Email);
                    if (existingUser == null)
                    {
                        var user = new IdentityUser { UserName = member.Email, Email = member.Email };
                        var result = await _userManager.CreateAsync(user, "Member@1234");
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, "Member");
                            member.UserId = user.Id;
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null) return NotFound();
            return View(member);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Members member)
        {
            if (id != member.Id) return NotFound();
            ModelState.Remove("Loans");
            ModelState.Remove("SavingsTransactions");
            ModelState.Remove("User");
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                _context.Update(member);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null) return NotFound();
            _context.Members.Remove(member);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}