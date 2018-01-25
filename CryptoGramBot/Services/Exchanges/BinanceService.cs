using Binance.Api;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using CryptoGramBot.Helpers.Convertors;
using CryptoGramBot.Models;
using CryptoGramBot.Services.Data;
using CryptoGramBot.Services.Exchanges.WebSockets.Binance;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoGramBot.Services.Exchanges
{
    public class BinanceService : IExchangeService
    {
        private readonly IBinanceApi _client;
        private readonly BinanceConfig _config;
        private readonly DatabaseService _databaseService;
        private readonly IBinanceWebsocketService _binanceWebsocketService;
        private readonly GeneralConfig _generalConfig;
        private readonly ILogger<BinanceService> _log;
        private readonly List<string> _symbols = new List<string>();

        public BinanceService(BinanceConfig config,
            DatabaseService databaseService,
            IBinanceWebsocketService binanceWebsocketService,
            GeneralConfig generalConfig,
            IBinanceApi binanceApi,
            ILogger<BinanceService> log)
        {
            _config = config;
            _databaseService = databaseService;
            _binanceWebsocketService = binanceWebsocketService;
            _generalConfig = generalConfig;
            _log = log;

            _client = binanceApi;
            _client.HttpClient.RateLimiter.Configure(TimeSpan.FromMinutes(1), 200);
        }

        public async Task<BalanceInformation> GetBalance()
        {
            List<WalletBalance> balances;
            try
            {
                var accountInfo = await _binanceWebsocketService.GetAccountInfoAsync();

                balances = BinanceConverter.BinanceToWalletBalances(accountInfo.Balances);
            }
            catch (Exception e)
            {
                _log.LogError("Error in getting balances from binance: " + e.Message);
                throw;
            }

            var totalBtcBalance = 0m;
            foreach (var balance in balances)
            {
                decimal price;
                decimal btcAmount;
                decimal averagePrice = 0m;

                if (balance.Currency == _generalConfig.TradingCurrency)
                {
                    btcAmount = balance.Balance;
                    price = 0m;
                }
                else if (balance.Currency == "USDT")
                {
                    var marketPrice = await GetPrice("USDT", _generalConfig.TradingCurrency);
                    btcAmount = balance.Balance / marketPrice;
                    price = 0m;
                }
                else
                {
                    var marketPrice = await GetPrice(_generalConfig.TradingCurrency, balance.Currency);
                    price = marketPrice;
                    btcAmount = (price * balance.Balance);
                    averagePrice =
                        await _databaseService.GetBuyAveragePrice(_generalConfig.TradingCurrency, balance.Currency, Constants.Binance, balance.Balance);
                }

                balance.PercentageChange = ProfitCalculator.PriceDifference(price, averagePrice);
                balance.BtcAmount = btcAmount;
                balance.Price = price;
                totalBtcBalance = totalBtcBalance + balance.BtcAmount;
            }

            var lastBalance = await _databaseService.GetBalance24HoursAgo(Constants.Binance);

            var dollarAmount = await GetDollarAmount(_generalConfig.TradingCurrency, totalBtcBalance);

            var currentBalance = await _databaseService.AddBalance(totalBtcBalance, dollarAmount, Constants.Binance);
            await _databaseService.AddWalletBalances(balances);

            return new BalanceInformation(currentBalance, lastBalance, Constants.Binance, balances);
        }

        public async Task<decimal> GetDollarAmount(string baseCcy, decimal btcAmount)
        {
            if (baseCcy == "USDT")
            {
                return Math.Round(btcAmount, 3);
            }

            var price = await GetPrice("USDT", baseCcy);
            return Math.Round(price * btcAmount, 3);
        }

        public async Task<List<Deposit>> GetNewDeposits()
        {
            var list = new List<Deposit>();

            try
            {
                var binanceClient = GetApi();
                using (var user = new BinanceApiUser(_config.Key, _config.Secret))
                {
                    var binanceDesposits = await binanceClient.GetDepositsAsync(user, _generalConfig.TradingCurrency);
                    list = BinanceConverter.BinanceToDeposits(binanceDesposits);
                }
            }
            catch (Exception e)
            {
                _log.LogError("Error in getting deposits from Binance: " + e.Message);
            }

            var newDeposits = await _databaseService.AddDeposits(list, Constants.Binance);

            await _databaseService.AddLastChecked("Binance.DepositCheck", DateTime.Now);

            return newDeposits;
        }

        public async Task<List<OpenOrder>> GetNewOpenOrders(DateTime lastChecked)
        {
            var openOrders = new List<OpenOrder>();

            try
            {
                foreach (var symbol in _symbols)
                {
                    var response = await _binanceWebsocketService.GetOpenOrdersAsync(symbol);
                    var ccy2 = symbol.Remove(symbol.Length - _generalConfig.TradingCurrency.Length);
                    var list = BinanceConverter.BinanceToOpenOrders(response, _generalConfig.TradingCurrency, ccy2);

                    openOrders.AddRange(list);
                }
            }
            catch (Exception e)
            {
                _log.LogError("Error in getting openOrders from binance: " + e.Message);
            }

            var newOrders = await _databaseService.AddOpenOrders(openOrders);

            return newOrders;
        }

        public async Task<List<Withdrawal>> GetNewWithdrawals()
        {
            // no api call
            var list = new List<Withdrawal>();

            try
            {
                var binanceClient = GetApi();

                using (var user = new BinanceApiUser(_config.Key, _config.Secret))
                {
                    var binanceDesposits = await binanceClient.GetWithdrawalsAsync(user, _generalConfig.TradingCurrency);
                    list = BinanceConverter.BinanceToWithdrawals(binanceDesposits);
                }
            }
            catch (Exception e)
            {
                _log.LogError("Error in getting withdrawals from binance: " + e.Message);
            }

            var newWithdrawals = await _databaseService.AddWithdrawals(list, Constants.Binance);
            await _databaseService.AddLastChecked("Binance.WithdrawalCheck", DateTime.Now);
            return newWithdrawals;
        }

        public async Task<List<Trade>> GetOrderHistory(DateTime lastChecked)
        {
            var list = new List<Trade>();

            try
            {
                foreach (var symbol in _symbols)
                {
                    var response = await _binanceWebsocketService.GetAccountTradesAsync(symbol);
                    var ccy2 = symbol.Remove(symbol.Length - _generalConfig.TradingCurrency.Length);

                    var symlist = BinanceConverter.BinanceToTrades(response, _generalConfig.TradingCurrency, ccy2, _log);
                    list.AddRange(symlist);
                }
            }
            catch (Exception e)
            {
                _log.LogError("Error in getting trades from binance: " + e.Message);
            }

            return list;
        }

        public async Task<decimal> GetPrice(string baseCcy, string termsCurrency)
        {
            var sym = await _binanceWebsocketService.GetPriceAsync($"{termsCurrency}{baseCcy}");

            if (sym != null)
            {
                return sym.Value;
            }

            return decimal.Zero;
        }

        public async Task GetSymbols()
        {
            _symbols.Clear();

            var symbols = await _binanceWebsocketService.GetSymbolsAsync();

            foreach (var response in symbols)
            {
                if (response.QuoteAsset.Equals(_generalConfig.TradingCurrency))
                {
                    _symbols.Add($"{response.BaseAsset}{response.QuoteAsset}");
                }
            }
        }

        private IBinanceApi GetApi()
        {
            return _client;
        }
    }
}