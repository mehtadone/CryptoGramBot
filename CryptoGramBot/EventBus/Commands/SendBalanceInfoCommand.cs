using System.Collections.Generic;
using CryptoGramBot.Models;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Commands
{
    public class SendBalanceInfoCommand : ICommand
    {
        public SendBalanceInfoCommand(
            BalanceHistory current,
            BalanceHistory lastBalance,
            IEnumerable<WalletBalance> walletBalances,
            string accountName)
        {
            Current = current;
            LastBalance = lastBalance;
            WalletBalances = walletBalances;
            AccountName = accountName;
        }

        public string AccountName { get; }
        public BalanceHistory Current { get; }
        public BalanceHistory LastBalance { get; }
        public IEnumerable<WalletBalance> WalletBalances { get; }
    }
}