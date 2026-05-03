namespace MINI_SACCO_SYSTEM.Models
{
    public class LoanRepayment
    {
        public int Id { get; set; }
        public int LoanId { get; set; }
        public Loans Loan { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidOn { get; set; } = DateTime.Now;
        public string Notes { get; set; }
    }
}