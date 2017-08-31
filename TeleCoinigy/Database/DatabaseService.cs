using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using Microsoft.Extensions.Logging;
using TeleCoinigy.Helpers;
using TeleCoinigy.Models;
using Logger = Serilog.Core.Logger;

namespace TeleCoinigy.Database
{
    public class DatabaseService
    {
        private readonly LiteRepository _db;
        private readonly Dictionary<string, BalanceHistory> _lastBalances = new Dictionary<string, BalanceHistory>();
        private readonly ILogger<DatabaseService> _log;

        public DatabaseService(ILogger<DatabaseService> log)
        {
            _log = log;
            _db = new LiteRepository(Constants.DatabaseName);
        }

        public BalanceHistory AddBalance(decimal balance, decimal dollarAmount, string name)
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

        public void AddTrades(IEnumerable<Trade> trades, out List<Trade> newTrades)
        {
            newTrades = new List<Trade>();
            _log.LogInformation("Adding new trades to database");

            foreach (var trade in trades)
            {
                newTrades.Add(trade);
                _db.Upsert(trade);
            }
        }

        public BalanceHistory GetLastBalance(string name)
        {
            return !_lastBalances.ContainsKey(name) ? GetLastBalanceFromDatabase(name) : _lastBalances[name];
        }

        public IEnumerable<Trade> GetTradesForPair(string ccy1, string ccy2)
        {
            var enumerable = _db.Query<Trade>()
                .Where(x => x.Base == ccy1 && x.Terms == ccy2)
                .ToEnumerable();

            return enumerable;
        }

        public void SaveProfitAndLoss(ProfitAndLoss pnl)
        {
            _log.LogInformation($"Adding pnl for {pnl.Pair} to database");
            _db.Upsert(pnl);
        }

        private void EnsureIndex()
        {
            var tradeCollection = _db.Database.GetCollection<Trade>();
            tradeCollection.EnsureIndex(x => x.Id);

            var profitCollection = _db.Database.GetCollection<ProfitAndLoss>();
            profitCollection.EnsureIndex(x => x.Pair);
        }

        private BalanceHistory GetLastBalanceFromDatabase(string name)
        {
            var balanceHistories = _db.Query<BalanceHistory>()
                .Where(x => x.Name == name)
                .ToEnumerable();

            var orderByDescending = balanceHistories.OrderByDescending(x => x.DateTime);

            _log.LogInformation($"Retrieving previous balance from database for: {name}");

            var lastBalance = orderByDescending.FirstOrDefault();

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
            var balances = _db.Insert(balanceHistory);
            _log.LogInformation($"Saved new balance in database for: {name}");
            _log.LogInformation("Adding balance to cache");
            _lastBalances[name] = balanceHistory;
        }
    }
}