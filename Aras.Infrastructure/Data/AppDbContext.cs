using Aras.Domain;
using Microsoft.EntityFrameworkCore;

namespace Aras.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.NationalCode).IsUnique();
            entity.Property(x => x.NationalCode).HasMaxLength(20).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.FatherName).HasMaxLength(100);
            entity.Property(x => x.BirthCertificationNumber).HasMaxLength(50);
            entity.Property(x => x.RegisterationNumber).HasMaxLength(50);
            entity.Property(x => x.BranchName).HasMaxLength(150);
            entity.Property(x => x.MobileNumber).HasMaxLength(20);
            entity.HasOne(x => x.Wallet).WithOne(x => x.Customer).HasForeignKey<Wallet>(x => x.CustomerId);
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.CustomerId).IsUnique();
            entity.Property(x => x.Balance).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.Status, x.CreatedAtUtc });
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.HasOne(x => x.Customer).WithMany(x => x.Orders).HasForeignKey(x => x.CustomerId);
        });

        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.OrderId).IsUnique();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.HasOne(x => x.Wallet).WithMany(x => x.Transactions).HasForeignKey(x => x.WalletId);
            entity.HasOne(x => x.Order).WithOne(x => x.WalletTransaction).HasForeignKey<WalletTransaction>(x => x.OrderId);
        });
    }
}
