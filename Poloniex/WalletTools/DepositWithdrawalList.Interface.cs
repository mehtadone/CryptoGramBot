using System.Collections.Generic;

namespace Poloniex.WalletTools
{
    public interface IDepositWithdrawalList
    {
        IList<Deposit> Deposits { get; }

        IList<Withdrawal> Withdrawals { get; }
    }
}