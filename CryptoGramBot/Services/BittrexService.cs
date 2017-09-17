using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Bittrex;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using Enexure.MicroBus;

namespace CryptoGramBot.Services
{
    public class BittrexService : IExchangeService
    {
        private readonly IMicroBus _bus;
        private readonly BittrexConfig _config;
        private readonly DatabaseService _databaseService;
        private readonly IExchange _exchange;
        private readonly PriceService _priceService;

        public BittrexService(
            BittrexConfig config,
            DatabaseService databaseService,
            PriceService priceService,
            IExchange exchange,
            IMicroBus bus)
        {
            _config = config;
            _databaseService = databaseService;
            _priceService = priceService;
            _exchange = exchange;
            _bus = bus;
            var context = new ExchangeContext
            {
                QuoteCurrency = "BTC",
                Simulate = false,
                ApiKey = config.Key,
                Secret = config.Secret
            };

            exchange.Initialise(context);
        }

        public async Task<BalanceInformation> GetBalance()
        {
            var response = await _exchange.GetBalances();
            var bittrexBalances = TradeConverter.BittrexToWalletBalances(response);

            var totalBtcBalance = 0m;
            foreach (var balance in bittrexBalances)
            {
                if (balance.Balance == 0) continue;

                decimal price;
                decimal btcAmount;
                decimal boughtPrice = 0m;

                switch (balance.Currency)
                {
                    case "BTC":
                        btcAmount = balance.Balance;
                        price = 1;
                        boughtPrice = 1;
                        break;

                    case "USDT":
                        price = await GetPrice(balance.Currency);
                        btcAmount = (balance.Balance / price);
                        var lastTradeForPair = _databaseService.GetLastTradeForPair(balance.Currency, Constants.Bittrex, TradeSide.Buy);
                        if (lastTradeForPair != null)
                        {
                            boughtPrice = lastTradeForPair.Limit;
                        }
                        break;

                    default:
                        price = await GetPrice(balance.Currency);
                        btcAmount = (price * balance.Balance);
                        var lastTradeForPair1 = _databaseService.GetLastTradeForPair(balance.Currency, Constants.Bittrex, TradeSide.Buy);
                        if (lastTradeForPair1 != null)
                        {
                            boughtPrice = lastTradeForPair1.Limit;
                        }
                        break;
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
                balance.BtcAmount = btcAmount;
                balance.Price = price;
                totalBtcBalance = totalBtcBalance + btcAmount;
            }

            var lastBalance = _databaseService.GetBalance24HoursAgo(Constants.Bittrex);
            var dollarAmount = await _priceService.GetDollarAmount(totalBtcBalance);
            var currentBalance = _databaseService.AddBalance(totalBtcBalance, dollarAmount, Constants.Bittrex);

            return new BalanceInformation(currentBalance, lastBalance, Constants.Bittrex, bittrexBalances);
        }

        public async Task<List<Trade>> GetOrderHistory(DateTime lastChecked)
        {
            var response = await _exchange.GetOrderHistory();
            var bittrexToTrades = TradeConverter.BittrexToTrades(response);
            return bittrexToTrades;
        }

        public async Task<decimal> GetPrice(string terms)
        {
            // USDT is not terms. But this bittrex library I'm using doesnt let me set it so checking via another method for the time being.
            if (terms == "USD" || terms == "USDT")
            {
                return await _priceService.GetDollarAmount(1);
            }

            var ticker = _exchange.GetTicker(terms);
            var price = ticker.Last.ToString();
            decimal priceAsDecimal;
            try
            {
                priceAsDecimal = decimal.Parse(price, NumberStyles.Float);
            }
            catch (Exception ex1)
            {
                try
                {
                    priceAsDecimal = await _priceService.GetPriceInBtc(terms);
                }
                catch (Exception ex2)
                {
                    return 0;
                }
            }
            return priceAsDecimal;
        }
    }
}