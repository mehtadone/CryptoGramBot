using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bittrex.Net;
using Bittrex.Net.Objects;
using CryptoExchange.Net.Authentication;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using CryptoGramBot.Services.Data;
using CryptoGramBot.Services.Pricing;
using Microsoft.Extensions.Logging;
using OpenOrder = CryptoGramBot.Models.OpenOrder;

namespace CryptoGramBot.Services.Exchanges
{
    public class BittrexService : IExchangeService
    {
        private readonly BittrexConfig _config;
        private readonly DatabaseService _databaseService;
        private readonly CryptoCompareApiService _cryptoCompareService;
        private readonly GeneralConfig _generalConfig;
        private readonly ILogger<BittrexService> _log;

        public BittrexService(
            BittrexConfig config,
            DatabaseService databaseService,
            CryptoCompareApiService cryptoCompareService,
            GeneralConfig generalConfig,
            ILogger<BittrexService> log)
        {
            _config = config;
            _databaseService = databaseService;
            _cryptoCompareService = cryptoCompareService;
            _generalConfig = generalConfig;
            _log = log;
        }

        public async Task<BalanceInformation> GetBalance()
        {
            List<WalletBalance> bittrexBalances = new List<WalletBalance>();
            try
            {
                using (var client = new BittrexClient(new BittrexClientOptions()
                {
                    ApiCredentials = new ApiCredentials(_config.Key, _config.Secret)
                }))
                {
                    var response = await client.GetBalancesAsync();

                    if (response.Success)
                    {
                        bittrexBalances = BittrexConvertor.BittrexToWalletBalances(response.Data);
                    }
                    else
                    {
                        _log.LogWarning($"Bittrex returned an error {response.Error.Code} : {response.Error.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                _log.LogError("Error in getting balances from Bittrex: " + e.Message);
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

            var reportingAmount = await GetReportingAmount(_generalConfig.TradingCurrency, totalBtcBalance, _generalConfig.ReportingCurrency);

            var currentBalance = await _databaseService.AddBalance(totalBtcBalance, reportingAmount, _generalConfig.ReportingCurrency, Constants.Bittrex);
            await _databaseService.AddWalletBalances(bittrexBalances);

            return new BalanceInformation(currentBalance, lastBalance, Constants.Bittrex, bittrexBalances);
        }

        public async Task<decimal> GetReportingAmount(string baseCcy, decimal baseAmount, string reportingCurrency)
        {
            // Bittrex supports USDT, use this for USD reporting
            if (reportingCurrency == "USD")
            {
                if (baseCcy == "USDT")
                {
                    return Math.Round(baseAmount, 3);
                }

                var price = await GetPrice("USDT", baseCcy);
                return Math.Round(price * baseAmount, 3);
            }
            else
            {
                // ReportingCurrency not supported by Bittrex - have to convert it externally
                var result = await _cryptoCompareService.GetReportingAmount(baseCcy, baseAmount, reportingCurrency);
                return result;
            }
        }

        public async Task<List<Deposit>> GetNewDeposits()
        {
            var list = new List<Deposit>();

            try
            {
                using (var client = new BittrexClient(new BittrexClientOptions()
                {
                    ApiCredentials = new ApiCredentials(_config.Key, _config.Secret)
                }))
                {
                    var response = await client.GetDepositHistoryAsync();

                    if (response.Success)
                    {
                        list = BittrexConvertor.BittrexToDeposits(response.Data);
                    }
                    else
                    {
                        _log.LogWarning($"Bittrex returned an error {response.Error.Code} : {response.Error.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                _log.LogError("Error in getting deposits from Bittrex: " + e.Message);
            }

            var newDeposits = await _databaseService.AddDeposits(list, Constants.Bittrex);

            await _databaseService.AddLastChecked("Bittrex.DepositCheck", DateTime.Now);
            return newDeposits;
        }

        public async Task<List<OpenOrder>> GetNewOpenOrders(DateTime lastChecked)
        {
            var openOrders = new List<OpenOrder>();

            try
            {
                using (var client = new BittrexClient(new BittrexClientOptions()
                {
                    ApiCredentials = new ApiCredentials(_config.Key, _config.Secret)
                }))
                {
                    var response = await client.GetOpenOrdersAsync();

                    if (response.Success)
                    {
                        openOrders = BittrexConvertor.BittrexToOpenOrders(response.Data);
                    }
                    else
                    {
                        _log.LogWarning($"Bittrex returned an error {response.Error.Code} : {response.Error.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                _log.LogError("Error in getting openOrders from Bittrex: " + e.Message);
            }

            var newOrders = await _databaseService.AddOpenOrders(openOrders);

            return newOrders;
        }

        public async Task<List<Withdrawal>> GetNewWithdrawals()
        {
            var list = new List<Withdrawal>();

            try
            {
                using (var client = new BittrexClient(new BittrexClientOptions()
                {
                    ApiCredentials = new ApiCredentials(_config.Key, _config.Secret)
                }))
                {
                    var response = await client.GetWithdrawalHistoryAsync();

                    if (response.Success)
                    {
                        list = BittrexConvertor.BittrexToWithdrawals(response.Data);
                    }
                    else
                    {
                        _log.LogWarning($"Bittrex returned an error {response.Error.Code} : {response.Error.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                _log.LogError("Error in getting withdrawals from Bittrex: " + e.Message);
            }

            var newWithdrawals = await _databaseService.AddWithdrawals(list, Constants.Bittrex);
            await _databaseService.AddLastChecked("Bittrex.WithdrawalCheck", DateTime.Now);
            return newWithdrawals;
        }

        public async Task<List<Trade>> GetOrderHistory(DateTime lastChecked)
        {
            var list = new List<Trade>();

            try
            {
                using (var client = new BittrexClient(new BittrexClientOptions()
                {
                    ApiCredentials = new ApiCredentials(_config.Key, _config.Secret)
                }))
                {
                    var response = await client.GetOrderHistoryAsync();

                    if (response.Success)
                    {
                        list = BittrexConvertor.BittrexToTrades(response.Data, _log);
                    }
                    else
                    {
                        _log.LogWarning($"Bittrex returned an error {response.Error.Code} : {response.Error.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                _log.LogError("Error in getting trades from Bittrex: " + e.Message);
            }

            return list;
        }

        public async Task<decimal> GetPrice(string baseCcy, string termsCurrency)
        {
            if (Helpers.Helpers.CurrenciesAreEquivalent(baseCcy, termsCurrency))
            {
                return 1;
            }
                
            BittrexPrice tick = null;
            try
            {
                tick = await GetTicker(baseCcy, termsCurrency);
            }
            catch (Exception e)
            {
                _log.LogError($"Error in getting {baseCcy}-{termsCurrency} ticker from Bittrex: {e.Message}");
            }

            if (tick != null && tick.Last != Decimal.Zero)
            {
                return tick.Last;
            }

            var btcPrice = await GetTicker(Constants.BTC, termsCurrency);

            if (btcPrice?.Last != null)
            {
                var btcBasePrice = await GetTicker(Constants.BTC, baseCcy);
                if (btcBasePrice?.Last != null)
                {
                    return btcPrice.Last * btcBasePrice.Last;
                }

                var baseBtcPrice = await GetTicker(baseCcy, Constants.BTC);

                if (baseBtcPrice?.Last != null)
                {
                    return baseBtcPrice.Last * btcPrice.Last;
                }
            }
            return 0;
        }

        private async Task<BittrexPrice> GetTicker(string baseCcy, string termsCurrency)
        {
            using (var client = new BittrexClient(new BittrexClientOptions()
            {
                ApiCredentials = new ApiCredentials(_config.Key, _config.Secret)
            }))
            {
                var response = await client.GetTickerAsync($"{baseCcy}-{termsCurrency}");

                if (response.Success)
                {
                    return response.Data;
                }

                _log.LogWarning($"Bittrex returned an error {response.Error.Code} : {response.Error.Message}");
                return null;
            }
        }
    }
}