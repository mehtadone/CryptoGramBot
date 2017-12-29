using System;

namespace Jojatekok.PoloniexAPI.WalletTools
{
    public interface IWithdrawal
    {
        ulong Id { get; }

        string Currency { get; }
        string Address { get; }
        double Amount { get; }

        DateTime Time { get; }
        string IpAddress { get; }

        string Status { get; }
    }
}
