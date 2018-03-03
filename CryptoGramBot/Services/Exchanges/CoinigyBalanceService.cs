using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using CryptoGramBot.Services.Data;
using CryptoGramBot.Services.Pricing;

namespace CryptoGramBot.Services.Exchanges
{
    public class CoinigyBalanceService : IBalanceService
    {
        private readonly CoinigyApiService _coinigyApiService;
        private readonly DatabaseService _databaseService;
        private readonly PriceService _priceService;
        private readonly GeneralConfig _config;

        public CoinigyBalanceService(
            CoinigyApiService coinigyApiService,
            PriceService priceService,
            DatabaseService databaseService,
            GeneralConfig config)
        {
            _coinigyApiService = coinigyApiService;
            _priceService = priceService;
            _databaseService = databaseService;
            _config = config;
        }

        public async Task<BalanceInformation> GetAccountBalance(int accountId)
        {
            var accounts = await _coinigyApiService.GetAccounts();
            var selectedAccount = accounts[accountId];

            var hour24Balance = await _databaseService.GetBalance24HoursAgo(selectedAccount.AuthId);
            var balanceCurrent = await _coinigyApiService.GetBtcBalance(selectedAccount.AuthId);
            var reportingAmount = await _priceService.GetReportingAmount(Constants.BTC, balanceCurrent, _config.ReportingCurrency, Constants.Bittrex);

            // Add to database. Should move these "Add to database" as an event which is called whenever a balance is queried
            var currentBalance = await _databaseService.AddBalance(balanceCurrent, reportingAmount, _config.ReportingCurrency, selectedAccount.AuthId);
            return new BalanceInformation(currentBalance, hour24Balance, selectedAccount.Name); ;
        }

        public async Task<Dictionary<int, CoinigyAccount>> GetAccounts()
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

        public async Task<BalanceInformation> GetBalance(string accountName)
        {
            var hour24Balance = await _databaseService.GetBalance24HoursAgo(accountName);
            var balanceCurrent = await _coinigyApiService.GetBtcBalance();
            var reportingAmount = await _priceService.GetReportingAmount(Constants.BTC, balanceCurrent, _config.ReportingCurrency, Constants.Bittrex);

            var currentBalance = await _databaseService.AddBalance(balanceCurrent, reportingAmount, _config.ReportingCurrency, accountName);
            return new BalanceInformation(currentBalance, hour24Balance, accountName);
        }

        public async Task<BalanceInformation> GetBalance()
        {
            var hour24Balance = await _databaseService.GetBalance24HoursAgo(Constants.TotalCoinigyBalance);
            var balanceCurrent = await _coinigyApiService.GetBtcBalance();
            var reportingAmount = await _priceService.GetReportingAmount(Constants.BTC, balanceCurrent, _config.ReportingCurrency, Constants.Bittrex);

            var currentBalance = await _databaseService.AddBalance(balanceCurrent, reportingAmount, _config.ReportingCurrency, Constants.TotalCoinigyBalance);
            return new BalanceInformation(currentBalance, hour24Balance, Constants.TotalCoinigyBalance);
        }
    }
}