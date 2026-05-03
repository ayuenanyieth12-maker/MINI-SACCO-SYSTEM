using System.ComponentModel.DataAnnotations;

namespace MINI_SACCO_SYSTEM.Models
{
    public class Loans
    {
        public int Id { get; set; }

        public int MemberId { get; set; }
        public Members Member { get; set; }

        public decimal Amount { get; set; }

        public decimal AmountRepaid { get; set; } = 0;

        [Required]
        public string Status { get; set; }

        public DateTime DateApplied { get; set; } = DateTime.Now;

        public DateTime? DueDate { get; set; }
        public decimal InterestRate { get; set; }

        public int LoanPeriodMonths { get; set; }

        public decimal InterestAmount { get; set; }

        public decimal TotalPayable { get; set; }

        public decimal MonthlyPayment { get; set; }
    }
}