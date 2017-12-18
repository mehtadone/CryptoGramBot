using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinanceExchange.API.Client;
using BinanceExchange.API.Models.Request;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using CryptoGramBot.Services.Pricing;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.Services.Exchanges
{
    public class BinanceService : IExchangeService
    {
        private readonly BinanceConfig _config;
        private readonly DatabaseService _databaseService;
        private readonly GeneralConfig _generalConfig;
        private readonly ILogger<BinanceService> _log;
        private readonly PriceService _priceService;

        public BinanceService(BinanceConfig config, DatabaseService databaseService,
            PriceService priceService,
            GeneralConfig generalConfig,
            ILogger<BinanceService> log)
        {
            _config = config;
            _databaseService = databaseService;
            _priceService = priceService;
            _generalConfig = generalConfig;
            _log = log;
        }

        public async Task<BalanceInformation> GetBalance()
        {
            List<WalletBalance> balances;
            try
            {
                var binanceClient = GetApi();
                var accountInfo = await binanceClient.GetAccountInformation();
                balances = TradeConverter.BinanceToWalletBalances(accountInfo.Balances);
            }
            catch (Exception e)
            {
                _log.LogError("Error in getting balances from bittrex: " + e.Message);
                throw;
            }

            var totalBtcBalance = 0m;
            foreach (var balance in balances)
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
                    var marketPrice = await _priceService.GetPrice("USDT", _generalConfig.TradingCurrency, Constants.Binance);
                    btcAmount = balance.Balance / marketPrice;
                    price = 0m;
                }
                else
                {
                    var marketPrice = await _priceService.GetPrice(_generalConfig.TradingCurrency, balance.Currency, Constants.Binance);
                    price = marketPrice;
                    btcAmount = (price * balance.Balance);
                    averagePrice =
                        await _databaseService.GetBuyAveragePrice(_generalConfig.TradingCurrency, balance.Currency, Constants.Binance, balance.Balance);
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

            var dollarAmount = await _priceService.GetDollarAmount(_generalConfig.TradingCurrency, totalBtcBalance, Constants.Binance);

            var currentBalance = await _databaseService.AddBalance(totalBtcBalance, dollarAmount, Constants.Bittrex);
            await _databaseService.AddWalletBalances(balances);

            return new BalanceInformation(currentBalance, lastBalance, Constants.Bittrex, balances);
        }

        public Task<decimal> GetDollarAmount(string baseCcy, decimal btcAmount)
        {
            throw new NotImplementedException();
        }

        public Task<List<Deposit>> GetNewDeposits()
        {
            // no api call
            return Task.FromResult(new List<Deposit>());
        }

        public Task<List<OpenOrder>> GetNewOpenOrders(DateTime lastChecked)
        {
            throw new NotImplementedException();
        }

        public Task<List<Withdrawal>> GetNewWithdrawals()
        {
            // no api call
            return Task.FromResult(new List<Withdrawal>());
        }

        public Task<List<Trade>> GetOrderHistory(DateTime lastChecked)
        {
            //            var api = GetApi();
            //            api.GetAccountTrades(new AllTradesRequest())
            return Task.FromResult(new List<Trade>());
        }

        public Task<decimal> GetPrice(string baseCcy, string termsCurrency)
        {
            throw new NotImplementedException();
        }

        private BinanceClient GetApi()
        {
            var client = new BinanceClient(new ClientConfiguration()
            {
                ApiKey = _config.Key,
                SecretKey = _config.Secret,
            });

            return client;
        }
    }
}