using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoGramBot.Data;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.Services.Data
{
    public class DatabaseService
    {
        private readonly CryptoGramBotDbContext _context;
        private readonly Dictionary<string, BalanceHistory> _lastBalances = new Dictionary<string, BalanceHistory>();
        private readonly ILogger<DatabaseService> _log;

        public DatabaseService(ILogger<DatabaseService> log, CryptoGramBotDbContext context)
        {
            _log = log;
            _context = context;
        }

        public async Task<BalanceHistory> AddBalance(decimal balance, decimal dollarAmount, string name)
        {
            var balanceHistory = new BalanceHistory
            {
                DateTime = DateTime.Now,
                Balance = balance,
                DollarAmount = dollarAmount,
                Name = name
            };

            _log.LogInformation($"Adding balance to database: {name} - {balance}");

            await SaveBalance(balanceHistory, name);

            return balanceHistory;
        }

        public async Task<List<Deposit>> AddDeposits(List<Deposit> deposits, string exchange)
        {
            _log.LogInformation($"Adding new deposits to database for {exchange}");

            var context = _context.Deposits;
            var list = new List<Deposit>();
            foreach (var deposit in deposits)
            {
                deposit.Exchange = exchange;
                var foundDeposit = context.FirstOrDefault(x => x.Currency == deposit.Currency &&
                                                                   x.Time == deposit.Time &&
                                                                   x.Exchange == deposit.Exchange &&
                                                                   x.Address == deposit.Address &&
                                                                   x.Amount == deposit.Amount &&
                                                                   x.TransactionId == deposit.TransactionId);

                if (foundDeposit == null)
                {
                    context.Add(deposit);
                    list.Add(deposit);
                }
            }

            await _context.SaveChangesAsync();
            return list;
        }

        public async Task AddLastChecked(string key, DateTime timestamp)
        {
            var lastCheckeds = _context.LastCheckeds;
            var lastChecked = lastCheckeds.FirstOrDefault(x => x.Exchange == key);

            if (lastChecked == null)
            {
                lastCheckeds.Add(new LastChecked
                {
                    Exchange = key,
                    Timestamp = timestamp
                });
            }
            else
            {
                lastChecked.Timestamp = timestamp;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<OpenOrder>> AddOpenOrders(List<OpenOrder> orders)
        {
            var firstOrder = orders.FirstOrDefault();

            _log.LogInformation(firstOrder != null
                ? $"Adding new open orders to database for {firstOrder.Exchange}"
                : "Adding new open orders to database");


            var context = _context.OpenOrders;

            var list = new List<OpenOrder>();
            foreach (var openOrder in orders)
            {
                var foundOpenOrder = context.FirstOrDefault(x => x.Terms == openOrder.Terms &&
                                                                   x.Base == openOrder.Base &&
                                                                   x.Exchange == openOrder.Exchange &&
                                                                   x.OrderUuid == openOrder.OrderUuid &&
                                                                   x.Price == openOrder.Price);

                if (foundOpenOrder == null)
                {
                    context.Add(openOrder);
                    list.Add(openOrder);
                }
            }

            await _context.SaveChangesAsync();
            return list;
        }

        public async Task<List<Trade>> AddTrades(IEnumerable<Trade> trades)
        {
            var newTrades = new List<Trade>();

            foreach (var trade in trades)
            {
                var foundTrade = _context.Trades.FirstOrDefault(
                    x => x.TimeStamp == trade.TimeStamp &&
                         x.Base == trade.Base &&
                         x.Exchange == trade.Exchange &&
                         x.Quantity == trade.Quantity &&
                         x.QuantityRemaining == trade.QuantityRemaining &&
                         x.Terms == trade.Terms &&
                         x.Cost == trade.Cost &&
                         x.ExchangeId == trade.ExchangeId

                );

                if (foundTrade == null)
                {
                    _context.Trades.Add(trade);
                    newTrades.Add(trade);
                }
            }

            await _context.SaveChangesAsync();

            var first = newTrades.FirstOrDefault();

            _log.LogInformation(first != null
                ? $"Added {newTrades.Count} new trades to database for {first.Exchange}"
                : $"Added {newTrades.Count} new trades to database");

            return newTrades;
        }

        public async Task AddWalletBalances(List<WalletBalance> walletBalances)
        {
            var walletBalancesDb = _context.WalletBalances;
            walletBalancesDb.AddRange(walletBalances);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Withdrawal>> AddWithdrawals(List<Withdrawal> withdrawals, string exchange)
        {
            _log.LogInformation($"Adding new withdrawals to database for {exchange}");
            var context = _context.Withdrawals;

            var list = new List<Withdrawal>();
            foreach (var withdrawal in withdrawals)
            {
                withdrawal.Exchange = exchange;
                var foundWithdrawal = context.FirstOrDefault(x => x.Currency == withdrawal.Currency &&
                                                                   x.Time == withdrawal.Time &&
                                                                   x.Exchange == withdrawal.Exchange &&
                                                                   x.Address == withdrawal.Address &&
                                                                   x.Amount == withdrawal.Amount &&
                                                                   x.TransactionId == withdrawal.TransactionId);

                if (foundWithdrawal == null)
                {
                    context.Add(withdrawal);
                    list.Add(withdrawal);
                }
            }

            await _context.SaveChangesAsync();
            return list;
        }

        public async Task DeleteAllTrades(string exchange)
        {
            _log.LogInformation($"Deleting trades for {exchange}");
            var trades = await _context.Trades.Where(x => x.Exchange == exchange).ToListAsync();
            _context.Trades.RemoveRange(trades);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<BalanceHistory>> GetAllBalances()
        {
            var all = await _context.BalanceHistories.ToListAsync();
            return all;
        }

        public async Task<IEnumerable<LastChecked>> GetAllLastChecked()
        {
            var all = await _context.LastCheckeds.ToListAsync();
            return all;
        }

        public async Task<IEnumerable<Currency>> GetAllPairs()
        {
            var collection = _context.Trades.AsQueryable();
            var distinct = await collection.GroupBy(g => new { g.Base, g.Terms })
                .Select(g => g.First())
                .ToListAsync();

            return distinct.Select(trade => new Currency
            {
                Base = trade.Base,
                Terms = trade.Terms
            })
                .ToList();
        }

        public async Task<IEnumerable<ProfitAndLoss>> GetAllProfitAndLoss()
        {
            var all = await _context.ProfitAndLosses.ToListAsync();
            return all;
        }

        public async Task<IEnumerable<Trade>> GetAllTrades()
        {
            var all = await _context.Trades.ToListAsync();
            return all;
        }

        //        public async Task<List<Trade>> GetAllTradesBuyFor(string currency, string exchange)
        //        {
        //            var collection = _context.Trades;
        //            var trades = await collection.Where(x => x.Terms == currency && x.Exchange == exchange && x.Side == TradeSide.Buy).ToListAsync();
        //            return trades;
        //        }

        public List<Trade> GetAllTradesFor(Currency currency)
        {
            var collection = _context.Trades;
            var trades = collection.Where(x => x.Terms == currency.Terms && x.Base == currency.Base).AsEnumerable();
            return trades.ToList();
        }

        public async Task<BalanceHistory> GetBalance24HoursAgo(string name)
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

                var collection = _context.BalanceHistories;
                var balanceHistories = await collection.Where(x => x.Name == name).OrderByDescending(x => x.DateTime).ToListAsync();

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

        public async Task<decimal> GetBuyAveragePrice(string ccy1, string ccy2, string exchange, decimal quantity)
        {
            var contextTrades = _context.Trades;
            var onlyBuys = await contextTrades
                .Where(x => x.Base == ccy1 && x.Terms == ccy2 && x.Exchange == exchange && x.Side == TradeSide.Buy)
                .OrderByDescending(x => x.TimeStamp)
                .ToListAsync();

            var price = ProfitCalculator.GetAveragePrice(onlyBuys, quantity);

            return price;
        }

        //
        //        public async Task<List<Trade>> GetBuysForPairAndQuantity(decimal sellPrice, string baseCcy, string terms)
        //        {
        //            var contextTrades = _context.Trades;
        //            var onlyBuys = await contextTrades
        //                .Where(x => x.Base == baseCcy && x.Terms == terms && x.Side == TradeSide.Buy)
        //                .OrderByDescending(x => x.TimeStamp)
        //                .ToListAsync();
        //
        //            return onlyBuys;
        //        }

        public async Task<DateTime?> GetLastBoughtAsync(string queryBaseCcy, string queryTerms, string exchange)
        {
            var lastBought = await _context.Trades.Where(x => x.Base == queryBaseCcy && x.Terms == queryTerms && x.Exchange == exchange && x.Side == TradeSide.Buy)
                .OrderByDescending(x => x.TimeStamp)
                .Select(x => x.TimeStamp)
                .FirstOrDefaultAsync();

            return lastBought;
        }

        public DateTime GetLastChecked(string key)
        {
            var contextLastCheckeds = _context.LastCheckeds;
            var lastChecked = contextLastCheckeds
                .FirstOrDefault(x => x.Exchange == key);

            return lastChecked?.Timestamp ?? DateTime.Now - TimeSpan.FromDays(30);
        }

        public Setting GetSetting(string name)
        {
            var allSettings = _context.Settings;
            var setting = allSettings.FirstOrDefault(x => x.Name == name);
            return setting;
        }

        public async Task<IEnumerable<Trade>> GetTradesForPair(string ccy1, string ccy2)
        {
            var contextTrades = _context.Trades;
            var enumerable = await contextTrades
                .Where(x => x.Base == ccy1 && x.Terms == ccy2)
                .ToListAsync();

            return enumerable;
        }

        public WalletBalance GetWalletBalance(string currency, string exchange)
        {
            var balances = _context.WalletBalances
                .Where(x => x.Currency == currency && x.Exchange == exchange)
                .OrderByDescending(x => x.Id);

            return balances.FirstOrDefault();
        }

        public async Task SaveProfitAndLoss(ProfitAndLoss pnl)
        {
            _log.LogInformation($"Adding pnl for {pnl.Pair} to database");

            var contextProfitAndLosses = _context.ProfitAndLosses;
            contextProfitAndLosses.Add(pnl);

            await _context.SaveChangesAsync();
        }

        public void SaveSetting(Setting setting)
        {
            var contextSettings = _context.Settings;

            var foundSetting = contextSettings.FirstOrDefault(x => x.Name == setting.Name);

            if (foundSetting == null)
            {
                _context.Settings.Add(setting);
            }
        }

        private async Task SaveBalance(BalanceHistory balanceHistory, string name)
        {
            var balanceHistories = _context.BalanceHistories;
            balanceHistory.Name = name;
            balanceHistories.Add(balanceHistory);
            _context.BalanceHistories.Add(balanceHistory);
            _log.LogInformation($"Saved new balance in database for: {name}");
            _lastBalances[name] = balanceHistory;

            await _context.SaveChangesAsync();
        }
    }
}