using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MINI_SACCO_SYSTEM.Services;

namespace MINI_SACCO_SYSTEM.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly NotificationService _notificationService;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationsController(NotificationService notificationService, UserManager<IdentityUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var type = User.IsInRole("Admin") ? "Admin" : _userManager.GetUserId(User);
            await _notificationService.MarkAllRead(type);
            var notifications = await _notificationService.GetNotifications(type);
            return View(notifications);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<JsonResult> GetUnreadCount()
        {
            var type = User.IsInRole("Admin") ? "Admin" : _userManager.GetUserId(User);
            var count = await _notificationService.GetUnreadCount(type);
            return Json(new { count });
        }
    }
}