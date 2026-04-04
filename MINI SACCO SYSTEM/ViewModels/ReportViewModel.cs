namespace MINI_SACCO_SYSTEM.ViewModels
{
    public class ReportViewModel
    {
        public int TotalMembers { get; set; }
        public int NewMembersThisMonth { get; set; }

        public decimal TotalSavings { get; set; }
        public decimal DepositsThisMonth { get; set; }
        public decimal WithdrawalsThisMonth { get; set; }

        public int TotalActiveLoans { get; set; }
        public int OverdueLoans { get; set; }
        public int ClearedLoans { get; set; }
        public decimal TotalLoanBook { get; set; }
        public decimal TotalRepaid { get; set; }
    }
}