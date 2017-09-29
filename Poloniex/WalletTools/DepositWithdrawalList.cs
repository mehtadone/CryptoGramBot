using System.Collections.Generic;
using Newtonsoft.Json;

namespace Poloniex.WalletTools
{
    public class DepositWithdrawalList : IDepositWithdrawalList
    {
        [JsonProperty("deposits")]
        public IList<Deposit> Deposits { get; private set; }

        [JsonProperty("withdrawals")]
        public IList<Withdrawal> Withdrawals { get; private set; }
    }
}