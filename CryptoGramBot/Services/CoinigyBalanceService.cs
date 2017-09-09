using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;

namespace CryptoGramBot.Services
{
    public class CoinigyBalanceService : IBalanceService
    {
        private readonly CoinigyApiService _coinigyApiService;
        private readonly DatabaseService _databaseService;
        private readonly PriceService _priceService;

        public CoinigyBalanceService(
            CoinigyApiService coinigyApiService,
            PriceService priceService,
            DatabaseService databaseService)
        {
            _coinigyApiService = coinigyApiService;
            _priceService = priceService;
            _databaseService = databaseService;
        }

        public async Task<BalanceInformation> GetAccountBalance(int accountId)
        {
            var accounts = await _coinigyApiService.GetAccounts();
            var selectedAccount = accounts[accountId];

            var hour24Balance = _databaseService.GetBalance24HoursAgo(selectedAccount.AuthId, Constants.Coinigy);
            var balanceCurrent = await _coinigyApiService.GetBtcBalance(selectedAccount.AuthId);
            var dollarAmount = await _priceService.GetDollarAmount(balanceCurrent);

            // Add to database. Should move these "Add to database" as an event which is called whenever a balance is queried
            var currentBalance = _databaseService.AddBalance(balanceCurrent, dollarAmount, selectedAccount.AuthId, Constants.Coinigy);
            return new BalanceInformation(currentBalance, hour24Balance, selectedAccount.Name); ;
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

        public async Task<BalanceInformation> GetBalance(string accountName)
        {
            var hour24Balance = _databaseService.GetBalance24HoursAgo(accountName, Constants.Coinigy);
            var balanceCurrent = await _coinigyApiService.GetBtcBalance();
            var dollarAmount = await _priceService.GetDollarAmount(balanceCurrent);

            var currentBalance = _databaseService.AddBalance(balanceCurrent, dollarAmount, accountName, Constants.Coinigy);
            return new BalanceInformation(currentBalance, hour24Balance, accountName);
        }
    }
}