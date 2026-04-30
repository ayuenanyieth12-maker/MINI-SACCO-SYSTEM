using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MINI_SACCO_SYSTEM.Models
{
    public class Members
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Phone { get; set; }

        public string Email { get; set; }

        public DateTime DateJoined { get; set; } = DateTime.Now;

        // Links this member to their login account
        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }

        public ICollection<Savings> SavingsTransactions { get; set; }
        public ICollection<Loans> Loans { get; set; }
    }
}