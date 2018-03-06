using System.Collections.Generic;

namespace CryptoGramBot.Models
{
    public class BalanceInformation
    {
        public string AccountName;
        public BalanceHistory CurrentBalance;
        public BalanceHistory PreviousBalance;
        public IEnumerable<WalletBalance> WalletBalances;

        public BalanceInformation(BalanceHistory currentBalance, BalanceHistory previousBalance, string accountName, IEnumerable<WalletBalance> walletBalances = null)
        {
            CurrentBalance = currentBalance;
            PreviousBalance = previousBalance;
            AccountName = accountName;
            WalletBalances = walletBalances;
        }

        public int Id { get; set; }
    }
}