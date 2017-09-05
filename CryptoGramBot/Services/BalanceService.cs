using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.Database;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;

namespace CryptoGramBot.Services
{
    public class BalanceService
    {
        private readonly CoinigyApiService _coinigyApiService;
        private readonly DatabaseService _databaseService;

        public BalanceService(CoinigyApiService coinigyApiService, DatabaseService databaseService)
        {
            _coinigyApiService = coinigyApiService;
            _databaseService = databaseService;
        }

        public void AddTrades(IEnumerable<Trade> trades, out List<Trade> newTrades)
        {
            _databaseService.AddTrades(trades, out newTrades);
        }

        public async Task<BalanceInformation> Get24HourTotalBalance()
        {
            var hour24Balance = _databaseService.GetBalance24HoursAgo(Constants.CoinigyBalance);
            var balanceCurrent = await _coinigyApiService.GetBtcBalance();

            var dollarAmount = await GetDollarAmount(balanceCurrent);
            var currentBalance = _databaseService.AddBalance(balanceCurrent, dollarAmount, Constants.CoinigyBalance);
            return new BalanceInformation(currentBalance, hour24Balance, Constants.CoinigyBalance);
        }

        public async Task<BalanceInformation> GetAccountBalance(int accountId)
        {
            var accounts = await _coinigyApiService.GetAccounts();
            var selectedAccount = accounts[accountId];
            var balance = await _coinigyApiService.GetBtcBalance(selectedAccount.AuthId);

            var dollarAmount = await GetDollarAmount(balance);

            var lastBalance = _databaseService.GetLastBalance(selectedAccount.Name);
            var currentBalance = _databaseService.AddBalance(balance, dollarAmount, selectedAccount.Name);

            return new BalanceInformation(currentBalance, lastBalance, selectedAccount.Name);
        }

        public async Task<BalanceInformation> GetAccountBalance24HoursAgo(int accountId)
        {
            var accounts = await _coinigyApiService.GetAccounts();
            var selectedAccount = accounts[accountId];
            var balance24HoursAgo = _databaseService.GetBalance24HoursAgo(selectedAccount.Name);
            var balanceCurrent = await _coinigyApiService.GetBtcBalance(selectedAccount.AuthId);

            var dollarAmount = await GetDollarAmount(balanceCurrent);
            var currentBalance = _databaseService.AddBalance(balanceCurrent, dollarAmount, selectedAccount.Name);
            return new BalanceInformation(currentBalance, balance24HoursAgo, selectedAccount.Name);
        }

        public async Task<Dictionary<int, Account>> GetAccounts()
        {
            var accounts = await _coinigyApiService.GetAccounts();
            return accounts;
        }

        public async Task<List<BalanceInformation>> GetAllBalances()
        {
            var balances = new List<BalanceInformation>();
            var accounts = await _coinigyApiService.GetAccounts();
            foreach (var account in accounts)
            {
                var accountBalance = await GetAccountBalance(account.Key);
                balances.Add(accountBalance);
            }

            return balances;
        }

        public async Task<ProfitAndLoss> GetPnLInfo(string ccy1, string ccy2)
        {
            var tradesForPair = _databaseService.GetTradesForPair(ccy1, ccy2);
            var profitAndLoss = ProfitCalculator.GetProfitAndLoss(tradesForPair, ccy1, ccy2);

            var dollarAmount = await GetDollarAmount(profitAndLoss.Profit);

            profitAndLoss.DollarProfit = dollarAmount;

            _databaseService.SaveProfitAndLoss(profitAndLoss);

            return profitAndLoss;
        }

        public async Task<BalanceInformation> GetTotalBalance()
        {
            var balance = await _coinigyApiService.GetBtcBalance();
            var lastBalance = _databaseService.GetLastBalance(Constants.CoinigyBalance);
            var dollarAmount = await GetDollarAmount(balance);
            var currentBalance = _databaseService.AddBalance(balance, dollarAmount, Constants.CoinigyBalance);

            return new BalanceInformation(currentBalance, lastBalance, Constants.CoinigyBalance);
        }

        private async Task<decimal> GetDollarAmount(decimal balance)
        {
            var lastBid = await _coinigyApiService.GetTicker("BTC/USD");
            return Math.Round(lastBid * balance, 2);
        }
    }
}