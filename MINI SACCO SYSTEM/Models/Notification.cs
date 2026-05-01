namespace MINI_SACCO_SYSTEM.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // "Admin" or member's UserId
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Link { get; set; } // where to go when clicked
    }
}
