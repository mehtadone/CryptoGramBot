using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BittrexSharp;
using BittrexSharp.Domain;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using Microsoft.Extensions.Logging;
using OpenOrder = CryptoGramBot.Models.OpenOrder;

namespace CryptoGramBot.Services.Exchanges
{
    public class BittrexService : IExchangeService
    {
        private readonly DatabaseService _databaseService;
        private readonly Bittrex _exchange;
        private readonly GeneralConfig _generalConfig;
        private readonly ILogger<BittrexService> _log;

        public BittrexService(
            BittrexConfig config,
            DatabaseService databaseService,
            GeneralConfig generalConfig,
            ILogger<BittrexService> log)
        {
            var config1 = config;
            _databaseService = databaseService;
            _generalConfig = generalConfig;
            _log = log;

            _exchange = new Bittrex(config1.Key, config1.Secret);
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
                        await _databaseService.GetBuyAveragePrice(_generalConfig.TradingCurrency, balance.Currency, Constants.Bittrex, balance.Balance);
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

            var lastBalance = await _databaseService.GetBalance24HoursAgo(Constants.Bittrex);

            var dollarAmount = await GetDollarAmount(_generalConfig.TradingCurrency, totalBtcBalance);

            var currentBalance = await _databaseService.AddBalance(totalBtcBalance, dollarAmount, Constants.Bittrex);
            await _databaseService.AddWalletBalances(bittrexBalances);

            return new BalanceInformation(currentBalance, lastBalance, Constants.Bittrex, bittrexBalances);
        }

        public async Task<List<Deposit>> GetNewDeposits()
        {
            var list = await _exchange.GetDepositHistory();

            var localDesposits = list.Select(Mapper.Map<Deposit>).ToList();
            var newDeposits = await _databaseService.AddDeposits(localDesposits, Constants.Bittrex);

            await _databaseService.AddLastChecked("Bittrex.DepositCheck", DateTime.Now);
            return newDeposits;
        }

        public async Task<List<OpenOrder>> GetNewOpenOrders(DateTime lastChecked)
        {
            var openOrders = await _exchange.GetOpenOrders();

            var orders = TradeConverter.BittrexToOpenOrders(openOrders);
            var newOrders = await _databaseService.AddOpenOrders(orders);

            return newOrders;
        }

        public async Task<List<Withdrawal>> GetNewWithdrawals()
        {
            var list = await _exchange.GetWithdrawalHistory();

            var localWithdrawals = list.Select(Mapper.Map<Withdrawal>).ToList();

            var newWithdrawals = await _databaseService.AddWithdrawals(localWithdrawals, Constants.Bittrex);
            await _databaseService.AddLastChecked("Bittrex.WithdrawalCheck", DateTime.Now);
            return newWithdrawals;
        }

        public async Task<List<CryptoGramBot.Models.Trade>> GetOrderHistory(DateTime lastChecked)
        {
            var response = await _exchange.GetOrderHistory();
            var bittrexToTrades = TradeConverter.BittrexToTrades(response, _log);
            return bittrexToTrades;
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

        public async Task<decimal> GetPrice(string baseCcy, string termsCurrency)
        {
            Ticker tcik = null;
            try
            {
                tcik = await _exchange.GetTicker(baseCcy, termsCurrency);
            }
            catch (Exception e)
            {
                _log.LogError("Error in getting ticker from bittrex: " + e.Message);
            }

            if (tcik != null && tcik.Last.HasValue)
            {
                return tcik.Last.Value;
            }

            var btcPrice = await _exchange.GetTicker("BTC", termsCurrency);

            if (btcPrice?.Last != null)
            {
                var btcBasePrice = await _exchange.GetTicker("BTC", baseCcy);
                if (btcBasePrice?.Last != null)
                {
                    return btcPrice.Last.Value * btcBasePrice.Last.Value;
                }
                else
                {
                    var baseBtcPrice = await _exchange.GetTicker(baseCcy, "BTC");

                    if (baseBtcPrice?.Last != null)
                    {
                        return baseBtcPrice.Last.Value * btcPrice.Last.Value;
                    }
                }
            }
            return 0;
        }
    }
}