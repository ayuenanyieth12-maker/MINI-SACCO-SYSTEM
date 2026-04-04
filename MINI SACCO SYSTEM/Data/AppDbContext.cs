using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MINI_SACCO_SYSTEM.Models;

namespace MINI_SACCO_SYSTEM.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Members> Members { get; set; }
        public DbSet<Savings> SavingsTransactions { get; set; }
        public DbSet<Loans> Loans { get; set; }
    }
}