using Microsoft.EntityFrameworkCore;
using MINI_SACCO_SYSTEM.Data;
using MINI_SACCO_SYSTEM.Models;

namespace MINI_SACCO_SYSTEM.Services
{
    public class NotificationService
    {
        private readonly AppDbContext _db;

        public NotificationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task NotifyAdmin(string message, string link = "/Dashboard")
        {
            _db.Notifications.Add(new Notification
            {
                Message = message,
                Type = "Admin",
                Link = link,
                IsRead = false,
                CreatedAt = DateTime.Now
            });
            await _db.SaveChangesAsync();
        }

        public async Task NotifyMember(string userId, string message, string link = "/MemberPortal")
        {
            _db.Notifications.Add(new Notification
            {
                Message = message,
                Type = userId,
                Link = link,
                IsRead = false,
                CreatedAt = DateTime.Now
            });
            await _db.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCount(string type)
        {
            return await _db.Notifications
                .Where(n => n.Type == type && !n.IsRead)
                .CountAsync();
        }

        public async Task<List<Notification>> GetNotifications(string type)
        {
            return await _db.Notifications
                .Where(n => n.Type == type)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToListAsync();
        }

        public async Task MarkAllRead(string type)
        {
            var notifications = await _db.Notifications
                .Where(n => n.Type == type && !n.IsRead)
                .ToListAsync();
            notifications.ForEach(n => n.IsRead = true);
            await _db.SaveChangesAsync();
        }
    }
}