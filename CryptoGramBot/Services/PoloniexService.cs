using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using Microsoft.Extensions.Logging;
using Poloniex;
using Poloniex.General;
using Poloniex.TradingTools;
using Poloniex.WalletTools;
using Deposit = CryptoGramBot.Models.Deposit;
using Trade = CryptoGramBot.Models.Trade;
using Withdrawal = CryptoGramBot.Models.Withdrawal;

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
            var price = await _priceService.GetPriceInBtc(terms);

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