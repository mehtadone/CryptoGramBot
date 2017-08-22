using System;
using System.Linq;
using LiteDB;
using TeleCoinigy.Models;
using Logger = Serilog.Core.Logger;

namespace TeleCoinigy.Database
{
    public class DatabaseService
    {
        private readonly Logger _log;

        public DatabaseService(Logger log)
        {
            _log = log;
        }

        public void AddBalance(double balance, string name)
        {
            var balanceHistory = new BalanceHistory
            {
                DateTime = DateTime.Now,
                Balance = balance
            };

            _log.Information($"Adding balance to database: {name} - {balance}");

            SaveBalance(balanceHistory, name);
        }

        public double GetLastBalance(string name)
        {
            using (var db = new LiteDatabase("database\\coinigyBalances.db"))
            {
                var balances = db.GetCollection<BalanceHistory>("balances");
                var histories = balances.Find(Query.All(Query.Descending), limit: 1)
                    .Where(x => x.Name == name);

                _log.Information($"Retrieving previous balance from database for: {name}");

                var balanceHistories = histories as BalanceHistory[] ?? histories.ToArray();

                return !balanceHistories.Any() ? 0 : balanceHistories[0].Balance;
            }
        }

        private void SaveBalance(BalanceHistory balanceHistory, string name)
        {
            balanceHistory.Name = name;
            using (var db = new LiteDatabase(@"coinigyBalances.db"))
            {
                var balances = db.GetCollection<BalanceHistory>("balances");
                balances.Insert(balanceHistory);
                _log.Information($"Saved new balance in database for: {name}");
            }
        }
    }
}