using System;
using System.Linq;
using LiteDB;
using TeleCoinigy.Models;
using Logger = Serilog.Core.Logger;

namespace TeleCoinigy.Database
{
    public class DatabaseService
    {
        private readonly LiteDatabase _db;
        private readonly Logger _log;

        public DatabaseService(Logger log)
        {
            _log = log;
            _db = new LiteDatabase(Constants.DatabaseName);
        }

        public BalanceHistory AddBalance(double balance, double dollarAmount, string name)
        {
            var balanceHistory = new BalanceHistory
            {
                DateTime = DateTime.Now,
                Balance = balance,
                DollarAmount = dollarAmount
            };

            _log.Information($"Adding balance to database: {name} - {balance}");

            SaveBalance(balanceHistory, name);

            return balanceHistory;
        }

        public BalanceHistory GetLastBalance(string name)
        {
            var balances = _db.GetCollection<BalanceHistory>("balances");
            var histories = balances.Find(Query.All(Query.Descending), limit: 1)
                .Where(x => x.Name == name);

            _log.Information($"Retrieving previous balance from database for: {name}");

            var balanceHistories = histories as BalanceHistory[] ?? histories.ToArray();

            var lastBalance = !balanceHistories.Any() ? new BalanceHistory() : balanceHistories[0];
            _log.Information($"Last balance for {name} was {lastBalance}");
            return lastBalance;
        }

        private void SaveBalance(BalanceHistory balanceHistory, string name)
        {
            balanceHistory.Name = name;
            var balances = _db.GetCollection<BalanceHistory>("balances");
            balances.Insert(balanceHistory);
            _log.Information($"Saved new balance in database for: {name}");
        }
    }
}