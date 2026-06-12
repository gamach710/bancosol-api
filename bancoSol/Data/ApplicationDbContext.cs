using bancoSol.Models;
using Microsoft.EntityFrameworkCore;
using Parameter = bancoSol.Models.Parameter;

namespace bancoSol.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<Parameter> Parameters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customer
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.CI).IsUnique();
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email).IsUnique();

            // Account
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.AccountNumber).IsUnique();
            modelBuilder.Entity<Account>()
                .Property(a => a.Balance)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Customer)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.CustomerId);

            // Transaction
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount).HasPrecision(18, 2);
            modelBuilder.Entity<Transaction>()
                .Property(t => t.PreviousBalance).HasPrecision(18, 2);
            modelBuilder.Entity<Transaction>()
                .Property(t => t.NewBalance).HasPrecision(18, 2);
            modelBuilder.Entity<Transaction>()
                .Property(t => t.ExchangeRate).HasPrecision(18, 6);
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId);

            // Transfer
            modelBuilder.Entity<Transfer>()
                .Property(t => t.SourceAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Transfer>()
                .Property(t => t.DestinationAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Transfer>()
                .Property(t => t.ExchangeRate).HasPrecision(18, 6);
            modelBuilder.Entity<Transfer>()
                .HasIndex(t => t.IdempotencyKey).IsUnique();
            modelBuilder.Entity<Transfer>()
                .HasOne(t => t.SourceAccount)
                .WithMany(a => a.TransfersOut)
                .HasForeignKey(t => t.SourceAccountId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Transfer>()
                .HasOne(t => t.DestinationAccount)
                .WithMany(a => a.TransfersIn)
                .HasForeignKey(t => t.DestinationAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Parameter
            modelBuilder.Entity<Parameter>()
                .HasIndex(p => new { p.Category, p.Code }).IsUnique();
        }
    }
}