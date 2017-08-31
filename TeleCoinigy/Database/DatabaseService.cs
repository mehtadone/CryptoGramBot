using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using Microsoft.Extensions.Logging;
using TeleCoinigy.Models;
using Logger = Serilog.Core.Logger;

namespace TeleCoinigy.Database
{
    public class DatabaseService
    {
        private readonly LiteDatabase _db;
        private readonly Dictionary<string, BalanceHistory> _lastBalances = new Dictionary<string, BalanceHistory>();
        private readonly ILogger<DatabaseService> _log;

        public DatabaseService(ILogger<DatabaseService> log)
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

            _log.LogInformation($"Adding balance to database: {name} - {balance}");

            SaveBalance(balanceHistory, name);

            return balanceHistory;
        }

        public BalanceHistory GetLastBalance(string name)
        {
            return !_lastBalances.ContainsKey(name) ? GetLastBalanceFromDatabase(name) : _lastBalances[name];
        }

        private BalanceHistory GetLastBalanceFromDatabase(string name)
        {
            var balances = _db.GetCollection<BalanceHistory>("balances");
            var histories = balances.Find(x => x.Name == name).OrderByDescending(x => x.DateTime);

            _log.LogInformation($"Retrieving previous balance from database for: {name}");

            var lastBalance = histories.FirstOrDefault(x => x.Name == name);

            if (lastBalance == null)
            {
                return new BalanceHistory();
            }

            _log.LogInformation($"Last balance for {name} was {lastBalance.Balance}");
            return lastBalance;
        }

        private void SaveBalance(BalanceHistory balanceHistory, string name)
        {
            balanceHistory.Name = name;
            var balances = _db.GetCollection<BalanceHistory>("balances");
            balances.Insert(balanceHistory);
            _log.LogInformation($"Saved new balance in database for: {name}");
            _log.LogInformation("Adding balance to cache");
            _lastBalances[name] = balanceHistory;
        }
    }
}