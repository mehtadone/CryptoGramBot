using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoGramBot.Models
{
    public class BalanceInformation
    {
        public string AccountName;

        public BalanceHistory CurrentBalance;

        public BalanceHistory PreviousBalance;

        public BalanceInformation(BalanceHistory currentBalance, BalanceHistory previousBalance, string accountName)
        {
            CurrentBalance = currentBalance;
            PreviousBalance = previousBalance;
            AccountName = accountName;
        }
    }
}