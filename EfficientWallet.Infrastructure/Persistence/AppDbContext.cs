using EfficientWallet.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ExchangeRate> ExchangeRates { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
    

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ExchangeRate>(e =>
            {
                e.Property(x => x.Currency).HasMaxLength(3).IsFixedLength();
                e.Property(x => x.Rate).HasPrecision(18, 6);
                e.HasIndex(x => new { x.RatesDate, x.Currency }).IsUnique();
            });

        }
    }
}
