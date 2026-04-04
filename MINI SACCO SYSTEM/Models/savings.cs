using System.ComponentModel.DataAnnotations;

namespace MINI_SACCO_SYSTEM.Models
{
    public class Savings
    {
        public int Id { get; set; }

        public int MemberId { get; set; }
        public Members Member { get; set; }

        [Required]
        public string Type { get; set; }

        public decimal Amount { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public string Notes { get; set; }
    }
}