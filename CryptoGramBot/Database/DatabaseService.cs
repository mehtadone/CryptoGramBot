using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using Microsoft.Extensions.Logging;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using Logger = Serilog.Core.Logger;

namespace CryptoGramBot.Database
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
            EnsureIndex();
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

        public void AddLastChecked(string exchange, DateTime timestamp)
        {
            var lastChecked = _db.SingleOrDefault<LastChecked>(x => x.Exchange == exchange);

            if (lastChecked == null)
            {
                _db.Insert(new LastChecked
                {
                    Exchange = exchange,
                    Timestamp = timestamp
                });
            }
            else
            {
                lastChecked.Timestamp = timestamp;
                var liteCollection = _db.Database.GetCollection<LastChecked>();
                liteCollection.Update(lastChecked);
            }
        }

        public void AddTrades(IEnumerable<Trade> trades, out List<Trade> newTrades)
        {
            newTrades = new List<Trade>();
            _log.LogInformation("Adding new trades to database");

            foreach (var trade in trades)
            {
                var singleOrDefault = _db.Fetch<Trade>().SingleOrDefault(x => x.Id == trade.Id);
                if (singleOrDefault == null)
                {
                    _db.Insert(trade);
                    newTrades.Add(trade);
                }
            }

            _log.LogInformation($"Added {newTrades.Count} new trades to database");
        }

        public IEnumerable<string> GetAllPairs()
        {
            var liteCollection = _db.Database.GetCollection<Trade>();
            var distinct = liteCollection.FindAll().ToList().Select(x => x.Terms).Distinct().OrderBy(x => x);
            return distinct;
        }

        public IEnumerable<Trade> GetAllTradesFor(string term)
        {
            var liteCollection = _db.Database.GetCollection<Trade>();
            var trades = liteCollection.Find(x => x.Terms == term);
            return trades;
        }

        public BalanceHistory GetBalance24HoursAgo(string name)
        {
            var dateTime = DateTime.Now - TimeSpan.FromHours(24);
            BalanceHistory hour24Balance;

            var histories = _lastBalances.Values.Where(x => x.DateTime.Hour == dateTime.Hour &&
                                                            x.DateTime.Day == dateTime.Day &&
                                                             x.DateTime.Month == dateTime.Month &&
                                                             x.DateTime.Year == dateTime.Year &&
                                                             x.Name == name)
                                                             .ToList();

            if (histories.Count == 0)
            {
                _log.LogInformation($"Retrieving 24 hour balance from database for: {name}");

                var liteCollection = _db.Database.GetCollection<BalanceHistory>();
                var balanceHistories = liteCollection.Find(x => x.Name == name).OrderByDescending(x => x.DateTime).ToList();

                histories = balanceHistories.FindAll(x => x.DateTime.Hour == dateTime.Hour &&
                                x.DateTime.Day == dateTime.Day &&
                                x.DateTime.Month == dateTime.Month &&
                                x.DateTime.Year == dateTime.Year)
                                .ToList();

                if (!histories.Any())
                {
                    _log.LogWarning($"Could not find a 24 hour balance for: {name}");
                    hour24Balance = new BalanceHistory
                    {
                        Balance = 0,
                        DollarAmount = 0,
                        Name = name
                    };
                    return hour24Balance;
                }
            }

            var orderByDescending = histories.OrderByDescending(x => x.DateTime);
            hour24Balance = orderByDescending.FirstOrDefault();

            return hour24Balance;
        }

        public BalanceHistory GetLastBalance(string name)
        {
            return !_lastBalances.ContainsKey(name) ? GetLastBalanceFromDatabase(name) : _lastBalances[name];
        }

        public DateTime GetLastChecked(string exchange)
        {
            var lastChecked = _db.Query<LastChecked>()
                .Where(x => x.Exchange == exchange)
                .SingleOrDefault();

            return lastChecked?.Timestamp ?? Constants.DateTimeUnixEpochStart;
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

            var lastCheckedCollection = _db.Database.GetCollection<LastChecked>();
            lastCheckedCollection.EnsureIndex(x => x.Exchange);
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
            _db.Insert(balanceHistory);
            _log.LogInformation($"Saved new balance in database for: {name}");
            _log.LogInformation("Adding balance to cache");
            _lastBalances[name] = balanceHistory;
        }
    }
}