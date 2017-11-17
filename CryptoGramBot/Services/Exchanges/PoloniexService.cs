using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using CryptoGramBot.Services.Pricing;
using Microsoft.Extensions.Logging;
using Poloniex;
using Poloniex.General;
using Poloniex.WalletTools;
using Deposit = CryptoGramBot.Models.Deposit;
using Trade = CryptoGramBot.Models.Trade;
using Withdrawal = CryptoGramBot.Models.Withdrawal;

namespace CryptoGramBot.Services.Exchanges
{
    public class PoloniexService : IExchangeService
    {
        private readonly DatabaseService _databaseService;
        private readonly GeneralConfig _generalConfig;
        private readonly ILogger<PoloniexService> _log;
        private readonly PoloniexClient _poloniexClient;
        private readonly PriceService _priceService;

        public PoloniexService(
            PoloniexConfig poloniexConfig,
            ILogger<PoloniexService> log,
            DatabaseService databaseService,
            GeneralConfig generalConfig,
            PriceService priceService)
        {
            _log = log;
            _databaseService = databaseService;
            _generalConfig = generalConfig;
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
                    var marketPrice = await _priceService.GetPrice("USDT", _generalConfig.TradingCurrency);
                    btcAmount = balance.Balance * marketPrice;
                    price = 0m;
                }
                else
                {
                    var marketPrice = await _priceService.GetPrice(_generalConfig.TradingCurrency, balance.Currency);
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
            var dollarAmount = await _priceService.GetDollarAmount(_generalConfig.TradingCurrency, totalBtcBalance);
            var currentBalance = await _databaseService.AddBalance(totalBtcBalance, dollarAmount, Constants.Poloniex);
            await _databaseService.AddWalletBalances(poloniexToWalletBalances);

            return new BalanceInformation(currentBalance, lastBalance, Constants.Poloniex, poloniexToWalletBalances);
        }

        public async Task<List<Deposit>> GetNewDeposits()
        {
            var checkedBefore = _databaseService.GetSetting("Poloniex.DepositCheck");
            var list = await GetDepositsAndWithdrawals(checkedBefore);
            var poloDeposits = list.Deposits;

            var localDesposits = poloDeposits.Select(Mapper.Map<Deposit>).ToList();

            var newDeposits = await _databaseService.AddDeposits(localDesposits, Constants.Poloniex);
            await _databaseService.AddLastChecked("Poloniex.DepositCheck", DateTime.Now);

            return newDeposits;
        }

        public async Task<List<OpenOrder>> GetNewOpenOrders(DateTime lastChecked)
        {
            var poloOrders = await _poloniexClient.Trading.GetOpenOrdersAsync();
            var orders = TradeConverter.PoloniexToOpenOrders(poloOrders);

            var newOrders = await _databaseService.AddOpenOrders(orders);

            return newOrders;
        }

        public async Task<List<Withdrawal>> GetNewWithdrawals()
        {
            var checkedBefore = _databaseService.GetSetting("Poloniex.WithdrawalCheck");
            var list = await GetDepositsAndWithdrawals(checkedBefore);
            var poloWithdrawals = list.Withdrawals;

            var withdrawals = poloWithdrawals.Select(Mapper.Map<Withdrawal>).ToList();

            var newWithdrawals = await _databaseService.AddWithdrawals(withdrawals, Constants.Poloniex);
            await _databaseService.AddLastChecked("Poloniex.WithdrawalCheck", DateTime.Now);

            return newWithdrawals;
        }

        public async Task<List<Trade>> GetOrderHistory(DateTime lastChecked)
        {
            var tradesAsync = await _poloniexClient.Trading.GetTradesAsync(CurrencyPair.All, lastChecked);
            var tradesAsyncResult = tradesAsync;

            var feeInfo = await _poloniexClient.Trading.GetFeeInfoAsync();

            var poloniexToTrades = TradeConverter.PoloniexToTrades(tradesAsyncResult, feeInfo);

            return poloniexToTrades;
        }

        private async Task<IDepositWithdrawalList> GetDepositsAndWithdrawals(Setting checkedBefore)
        {
            IDepositWithdrawalList depositWithdrawalList;
            if (checkedBefore == null || checkedBefore.Value == "false")
            {
                depositWithdrawalList = await _poloniexClient.Wallet.GetDepositsAndWithdrawalsAsync();
            }
            else
            {
                var lastChecked = _databaseService.GetLastChecked("Poloniex.DepositsAndWithdrawals");
                depositWithdrawalList =
                    await _poloniexClient.Wallet.GetDepositsAndWithdrawalsAsync(lastChecked, DateTime.MaxValue);
            }

            return depositWithdrawalList;
        }
    }
}