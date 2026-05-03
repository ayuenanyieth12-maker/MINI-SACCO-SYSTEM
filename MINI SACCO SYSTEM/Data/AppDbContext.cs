using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MINI_SACCO_SYSTEM.Models;

namespace MINI_SACCO_SYSTEM.Data
{
    public class AppDbContext : IdentityDbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Loans>(entity =>
            {
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.AmountRepaid).HasPrecision(18, 2);
                entity.Property(e => e.InterestAmount).HasPrecision(18, 2);
                entity.Property(e => e.InterestRate).HasPrecision(5, 2);
                entity.Property(e => e.TotalPayable).HasPrecision(18, 2);
                entity.Property(e => e.MonthlyPayment).HasPrecision(18, 2);
            });
        }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Members> Members { get; set; }
        public DbSet<Savings> SavingsTransactions { get; set; }
        public DbSet<Loans> Loans { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}