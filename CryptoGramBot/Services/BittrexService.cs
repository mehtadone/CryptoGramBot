using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bittrex;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.Services
{
    public class BittrexService : IExchangeService
    {
        private readonly IMicroBus _bus;
        private readonly BittrexConfig _config;
        private readonly DatabaseService _databaseService;
        private readonly IExchange _exchange;
        private readonly ILogger<BittrexService> _log;
        private readonly PriceService _priceService;

        public BittrexService(
            BittrexConfig config,
            DatabaseService databaseService,
            PriceService priceService,
            IExchange exchange,
            ILogger<BittrexService> log,
            IMicroBus bus)
        {
            _config = config;
            _databaseService = databaseService;
            _priceService = priceService;
            _exchange = exchange;
            _log = log;
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
            List<WalletBalance> bittrexBalances;
            try
            {
                var response = await _exchange.GetBalances();
                bittrexBalances = TradeConverter.BittrexToWalletBalances(response);
            }
            catch (Exception e)
            {
                _log.LogError("Error in getting balances from bittrex: " + e.Message);
                throw;
            }

            var totalBtcBalance = 0m;
            foreach (var balance in bittrexBalances)
            {
                if (balance.Balance == 0) continue;

                var marketPrice = await GetPrice(balance.Currency);

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
                        price = marketPrice;
                        btcAmount = (balance.Balance / price);
                        var lastTradeForPair =
                            _databaseService.GetLastTradeForPair(balance.Currency, Constants.Bittrex, TradeSide.Buy);
                        if (lastTradeForPair != null)
                        {
                            boughtPrice = lastTradeForPair.Limit;
                        }
                        break;

                    default:
                        price = marketPrice;
                        btcAmount = (price * balance.Balance);
                        var lastTradeForPair1 =
                            _databaseService.GetLastTradeForPair(balance.Currency, Constants.Bittrex, TradeSide.Buy);
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
                totalBtcBalance = totalBtcBalance + balance.BtcAmount;
            }

            var lastBalance = _databaseService.GetBalance24HoursAgo(Constants.Bittrex);
            var dollarAmount = await _priceService.GetDollarAmount(totalBtcBalance);
            var currentBalance = await _databaseService.AddBalance(totalBtcBalance, dollarAmount, Constants.Bittrex);
            await _databaseService.AddWalletBalances(bittrexBalances);

            return new BalanceInformation(currentBalance, lastBalance, Constants.Bittrex, bittrexBalances);
        }

        public async Task<List<Deposit>> GetNewDeposits()
        {
            var list = await _exchange.GetDeposits();

            var localDesposits = list.Select(Mapper.Map<Deposit>).ToList();
            var newDeposits = await _databaseService.AddDeposits(localDesposits, Constants.Bittrex);

            await _databaseService.AddLastChecked("Bittrex.DepositCheck", DateTime.Now);
            return newDeposits;
        }

        public async Task<List<Withdrawal>> GetNewWithdrawals()
        {
            var list = await _exchange.GetWithdrawals();

            var localWithdrawals = list.Select(Mapper.Map<Withdrawal>).ToList();

            var newWithdrawals = await _databaseService.AddWithdrawals(localWithdrawals, Constants.Bittrex);
            await _databaseService.AddLastChecked("Bittrex.WithdrawalCheck", DateTime.Now);
            return newWithdrawals;
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
            switch (terms)
            {
                case "USD":
                    return await _priceService.GetDollarAmount(1);

                case "USDT":
                    return await _priceService.GetDollarAmount(1);

                case "BTC":
                    return 0;
            }

            var ticker = await _exchange.GetTicker(terms);
            var price = ticker.Last.ToString();
            decimal priceAsDecimal;
            try
            {
                priceAsDecimal = decimal.Parse(price, NumberStyles.Float);
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