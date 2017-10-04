using System.IO;
using CryptoGramBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CryptoGramBot.Data
{
    public class CryptoGramBotDbContext : DbContext
    {
        public CryptoGramBotDbContext(DbContextOptions options) : base(options)
        { }

        public DbSet<BalanceHistory> BalanceHistories { get; set; }
        public DbSet<Deposit> Deposits { get; set; }
        public DbSet<LastChecked> LastCheckeds { get; set; }
        public DbSet<OpenOrder> OpenOrders { get; set; }
        public DbSet<ProfitAndLoss> ProfitAndLosses { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Trade> Trades { get; set; }
        public DbSet<WalletBalance> WalletBalances { get; set; }
        public DbSet<Withdrawal> Withdrawals { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Trade>().HasIndex(x => x.Id).IsUnique();
            builder.Entity<LastChecked>().HasIndex(x => x.Id).IsUnique();
            builder.Entity<ProfitAndLoss>().HasIndex(x => x.Id).IsUnique();
            builder.Entity<BalanceHistory>().HasIndex(x => x.Key).IsUnique();
            builder.Entity<WalletBalance>().HasIndex(x => x.Id).IsUnique();
            builder.Entity<Deposit>().HasIndex(x => x.Id).IsUnique();
            builder.Entity<Withdrawal>().HasIndex(x => x.Id).IsUnique();
            builder.Entity<OpenOrder>().HasIndex(x => x.Id).IsUnique();
            base.OnModelCreating(builder);
        }
    }

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CryptoGramBotDbContext>
    {
        public CryptoGramBotDbContext CreateDbContext(string[] args)
        {
            var databaseLocation = Directory.GetCurrentDirectory() + "/database/cryptogrambot.sqlite";

            var builder = new DbContextOptionsBuilder<CryptoGramBotDbContext>();

            builder.UseSqlite(databaseLocation);

            return new CryptoGramBotDbContext(builder.Options);
        }
    }
}