using System.Collections.Generic;

namespace Jojatekok.PoloniexAPI.WalletTools
{
    public interface IDepositWithdrawalList
    {
        IList<Deposit> Deposits { get; }

        IList<Withdrawal> Withdrawals { get; }
    }
}
