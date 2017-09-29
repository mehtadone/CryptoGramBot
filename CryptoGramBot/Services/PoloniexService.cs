using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using Microsoft.Extensions.Logging;
using Poloniex;
using Poloniex.General;
using Poloniex.MarketTools;
using Trade = CryptoGramBot.Models.Trade;

namespace CryptoGramBot.Services
{
    public class PoloniexService : IExchangeService
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<PoloniexService> _log;
        private readonly PoloniexClient _poloniexClient;
        private readonly PriceService _priceService;

        public PoloniexService(
            PoloniexConfig poloniexConfig,
            ILogger<PoloniexService> log,
            DatabaseService databaseService,
            PriceService priceService)
        {
            _log = log;
            _databaseService = databaseService;
            _priceService = priceService;
            _poloniexClient = new PoloniexClient(poloniexConfig.Key, poloniexConfig.Secret);
        }

        public async Task<BalanceInformation> GetBalance()
        {
            List<WalletBalance> poloniexToWalletBalances;
            try
            {
                var balances = await _poloniexClient.Wallet.GetBalancesAsync();
                poloniexToWalletBalances = TradeConverter.PoloniexToWalletBalances(balances);
            }
            catch (Exception e)
            {
                _log.LogError("Error in getting balances from poloniex: " + e.Message);
                throw;
            }

            var totalBtcBalance = 0m;
            foreach (var balance in poloniexToWalletBalances)
            {
                if (balance.BtcAmount == 0) continue;

                var price = await GetPrice(balance.Currency);
                var boughtPrice = 0m;

                var lastTradeForPair1 = _databaseService.GetLastTradeForPair(balance.Currency, Constants.Poloniex, TradeSide.Buy);
                if (lastTradeForPair1 != null)
                {
                    boughtPrice = lastTradeForPair1.Limit;
                }

                try
                {
                    balance.PercentageChange = ProfitCalculator.PriceDifference(price, boughtPrice);
                }
                catch
                {
                    // There maybe a divide by 0 issue if we couldn't find the last trade. Its fine. Just print zero
                    balance.PercentageChange = 0;
                }
                balance.Price = price;
                totalBtcBalance = totalBtcBalance + balance.BtcAmount;
            }

            var lastBalance = _databaseService.GetBalance24HoursAgo(Constants.Poloniex);
            var dollarAmount = await _priceService.GetDollarAmount(totalBtcBalance);
            var currentBalance = _databaseService.AddBalance(totalBtcBalance, dollarAmount, Constants.Poloniex);

            return new BalanceInformation(currentBalance, lastBalance, Constants.Poloniex, poloniexToWalletBalances);
        }

        public async Task<List<Trade>> GetOrderHistory(DateTime lastChecked)
        {
            var tradesAsync = await _poloniexClient.Trading.GetTradesAsync(CurrencyPair.All, lastChecked);
            var tradesAsyncResult = tradesAsync;

            var feeInfo = await _poloniexClient.Trading.GetFeeInfoAsync();

            var poloniexToTrades = TradeConverter.PoloniexToTrades(tradesAsyncResult, feeInfo);
            return poloniexToTrades;
        }

        public async Task<decimal> GetPrice(string terms)
        {
            switch (terms)
            {
                case "USD":
                    return await _priceService.GetDollarAmount(1);

                case "USDT":
                    return await _priceService.GetDollarAmount(1);

                case "BTC":
                    return 0;
            }

            // REALLY?? There is no simple getTicker on the polo client???
            var ticker =
                await _poloniexClient.Markets.GetChartDataAsync(new CurrencyPair("BTC", terms), MarketPeriod.Minutes5, DateTime.Now - TimeSpan.FromMinutes(10), DateTime.Now);

            var price = ticker.Last().Close;
            decimal priceAsDecimal;
            try
            {
                priceAsDecimal = Convert.ToDecimal(price);
            }
            catch (Exception)
            {
                try
                {
                    priceAsDecimal = await _priceService.GetPriceInBtc(terms);
                }
                catch (Exception)
                {
                    return 0;
                }
            }
            return priceAsDecimal;
        }
    }
}