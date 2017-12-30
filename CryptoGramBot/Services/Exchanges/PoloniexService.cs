using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using CryptoGramBot.Services.Data;
using Jojatekok.PoloniexAPI;
using Jojatekok.PoloniexAPI.MarketTools;
using Jojatekok.PoloniexAPI.WalletTools;
using Microsoft.Extensions.Logging;
using Deposit = CryptoGramBot.Models.Deposit;
using IOrder = Jojatekok.PoloniexAPI.TradingTools.IOrder;
using Order = Jojatekok.PoloniexAPI.TradingTools.Order;
using Trade = CryptoGramBot.Models.Trade;
using Withdrawal = CryptoGramBot.Models.Withdrawal;

namespace CryptoGramBot.Services.Exchanges
{
    public class PoloniexService : IExchangeService
    {
        private readonly DatabaseService _databaseService;
        private readonly GeneralConfig _generalConfig;
        private readonly ILogger<PoloniexService> _log;
        private readonly IPoloniexClientFactory _poloniexClientFactory;
        private readonly PoloniexConfig _poloniexConfig;

        public PoloniexService(
            PoloniexConfig poloniexConfig,
            ILogger<PoloniexService> log,
            DatabaseService databaseService,
            GeneralConfig generalConfig,
            IPoloniexClientFactory poloniexClientFactory)
        {
            _poloniexConfig = poloniexConfig;
            _log = log;
            _databaseService = databaseService;
            _generalConfig = generalConfig;
            _poloniexClientFactory = poloniexClientFactory;
        }

        public async Task<BalanceInformation> GetBalance()
        {
            List<WalletBalance> poloniexToWalletBalances;
            try
            {
                IDictionary<string, Balance> balances = new Dictionary<string, Balance>();
                using (var poloClient = _poloniexClientFactory.CreateClient(_poloniexConfig.Key, _poloniexConfig.Secret))
                {
                    balances = await poloClient.Wallet.GetBalancesAsync();
                }

                poloniexToWalletBalances = PoloniexConvertor.PoloniexToWalletBalances(balances);
            }
            catch (Exception e)
            {
                _log.LogError("Error in getting balances from poloniex: " + e.Message);
                throw;
            }

            var totalBtcBalance = 0m;
            foreach (var balance in poloniexToWalletBalances)
            {
                if (balance.Balance == 0) continue;

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
                    btcAmount = balance.Balance * marketPrice;
                    price = 0m;
                }
                else
                {
                    var marketPrice = await GetPrice(_generalConfig.TradingCurrency, balance.Currency);
                    price = marketPrice;
                    btcAmount = (price * balance.Balance);
                    averagePrice =
                        await _databaseService.GetBuyAveragePrice(_generalConfig.TradingCurrency, balance.Currency, Constants.Poloniex, balance.Balance);
                }
                try
                {
                    balance.PercentageChange = ProfitCalculator.PriceDifference(price, averagePrice);
                }
                catch
                {
                    // There maybe a divide by 0 issue if we couldn't find the last trade. Its fine. Just print zero
                    balance.PercentageChange = 0;
                }
                balance.BtcAmount = btcAmount;
                balance.Price = price;
                totalBtcBalance = totalBtcBalance + balance.BtcAmount;
            }

            var lastBalance = await _databaseService.GetBalance24HoursAgo(Constants.Poloniex);
            var dollarAmount = await GetDollarAmount(_generalConfig.TradingCurrency, totalBtcBalance);
            var currentBalance = await _databaseService.AddBalance(totalBtcBalance, dollarAmount, Constants.Poloniex);
            await _databaseService.AddWalletBalances(poloniexToWalletBalances);

            return new BalanceInformation(currentBalance, lastBalance, Constants.Poloniex, poloniexToWalletBalances);
        }

        public async Task<decimal> GetDollarAmount(string baseCcy, decimal btcAmount)
        {
            if (baseCcy == "USDT")
            {
                return Math.Round(btcAmount, 2);
            }

            var price = await GetPrice("USDT", baseCcy);
            return Math.Round(price * btcAmount, 2);
        }

        public async Task<List<Deposit>> GetNewDeposits()
        {
            var checkedBefore = _databaseService.GetSetting("Poloniex.DepositCheck");
            var list = await GetDepositsAndWithdrawals(checkedBefore);
            var poloDeposits = list.Deposits;

            var localDesposits = PoloniexConvertor.PoloniexToDeposits(poloDeposits);

            var newDeposits = await _databaseService.AddDeposits(localDesposits, Constants.Poloniex);
            await _databaseService.AddLastChecked("Poloniex.DepositCheck", DateTime.Now);

            return newDeposits;
        }

        public async Task<List<OpenOrder>> GetNewOpenOrders(DateTime lastChecked)
        {
            Dictionary<string, List<Order>> poloOrders;

            try
            {
                using (var poloClient = _poloniexClientFactory.CreateClient(_poloniexConfig.Key, _poloniexConfig.Secret))
                {
                    poloOrders = await poloClient.Trading.GetOpenOrdersAsync();
                }
            }
            catch (Exception e)
            {
                _log.LogError("Error in getting balances from poloniex: " + e.Message);
                throw;
            }

            var orders = PoloniexConvertor.PoloniexToOpenOrders(poloOrders);

            var newOrders = await _databaseService.AddOpenOrders(orders);

            return newOrders;
        }

        public async Task<List<Withdrawal>> GetNewWithdrawals()
        {
            var checkedBefore = _databaseService.GetSetting("Poloniex.WithdrawalCheck");
            var list = await GetDepositsAndWithdrawals(checkedBefore);
            var poloWithdrawals = list.Withdrawals;

            var withdrawals = PoloniexConvertor.PoloniexToWithdrawals(poloWithdrawals);

            var newWithdrawals = await _databaseService.AddWithdrawals(withdrawals, Constants.Poloniex);
            await _databaseService.AddLastChecked("Poloniex.WithdrawalCheck", DateTime.Now);

            return newWithdrawals;
        }

        public async Task<List<Trade>> GetOrderHistory(DateTime lastChecked)
        {
            try
            {
                using (var poloClient = _poloniexClientFactory.CreateClient(_poloniexConfig.Key, _poloniexConfig.Secret))
                {
                    var tradesAsync = await poloClient.Trading.GetTradesAsync(CurrencyPair.All, lastChecked, DateTime.Now);
                    var poloniexToTrades = PoloniexConvertor.PoloniexToTrades(tradesAsync);
                    return poloniexToTrades;
                }
            }
            catch (Exception e)
            {
                _log.LogError("Error in getting balances from poloniex: " + e.Message);
                throw;
            }
        }

        public async Task<decimal> GetPrice(string baseCcy, string termsCurrency)
        {
            IDictionary<CurrencyPair, IMarketData> ccyPairsData;

            try
            {
                using (var poloClient = _poloniexClientFactory.CreateClient(_poloniexConfig.Key, _poloniexConfig.Secret))
                {
                    ccyPairsData = await poloClient.Markets.GetSummaryAsync();
                }
            }
            catch (Exception e)
            {
                _log.LogError("Error in getting market summary from poloniex: " + e.Message);
                return 0;
            }

            var ccyPair = new CurrencyPair(baseCcy, termsCurrency);
            if (ccyPairsData.TryGetValue(ccyPair, out var mktData))
            {
                return (decimal)mktData.PriceLast;
            }

            var btcPricePair = new CurrencyPair(Constants.BTC, termsCurrency);
            if (ccyPairsData.TryGetValue(btcPricePair, out var btcPriceData))
            {
                var btcBasePricePair = new CurrencyPair(Constants.BTC, baseCcy);
                if (ccyPairsData.TryGetValue(btcBasePricePair, out var btcBasePriceData))
                {
                    return (decimal)(btcPriceData.PriceLast * btcBasePriceData.PriceLast);
                }
                else
                {
                    var baseBtcPricePair = new CurrencyPair(baseCcy, Constants.BTC);
                    if (ccyPairsData.TryGetValue(baseBtcPricePair, out var baseBtcPriceData))
                    {
                        return (decimal)(baseBtcPriceData.PriceLast * btcPriceData.PriceLast);
                    }
                }
            }

            return 0;
        }

        private async Task<IDepositWithdrawalList> GetDepositsAndWithdrawals(Setting checkedBefore)
        {
            IDepositWithdrawalList depositWithdrawalList;

            using (var poloClient = _poloniexClientFactory.CreateClient(_poloniexConfig.Key, _poloniexConfig.Secret))
            {
                if (checkedBefore == null || checkedBefore.Value == "false")
                {
                    depositWithdrawalList = await poloClient.Wallet.GetDepositsAndWithdrawalsAsync();
                }
                else
                {
                    var lastChecked = _databaseService.GetLastChecked("Poloniex.DepositsAndWithdrawals");
                    depositWithdrawalList =
                        await poloClient.Wallet.GetDepositsAndWithdrawalsAsync(lastChecked, DateTime.MaxValue);
                }
            }

            return depositWithdrawalList;
        }
    }
}