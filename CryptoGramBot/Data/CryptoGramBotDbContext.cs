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
        public DbSet<LastChecked> LastCheckeds { get; set; }
        public DbSet<ProfitAndLoss> ProfitAndLosses { get; set; }
        public DbSet<Trade> Trades { get; set; }
        public DbSet<WalletBalance> WalletBalances { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //                builder.Entity<DataEventRecord>().HasKey(m => m.Id);
            base.OnModelCreating(builder);
        }
    }

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CryptoGramBotDbContext>
    {
        public CryptoGramBotDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var databaseLocation = Directory.GetCurrentDirectory() + "/database/cryptogrambot.sqlite";

            var builder = new DbContextOptionsBuilder<CryptoGramBotDbContext>();

            builder.UseSqlite(databaseLocation);

            return new CryptoGramBotDbContext(builder.Options);
        }
    }
}