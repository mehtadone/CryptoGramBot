using System;

namespace Poloniex.WalletTools
{
    public interface IWithdrawal
    {
        string Address { get; }
        double Amount { get; }
        string Currency { get; }
        ulong Id { get; }
        string IpAddress { get; }
        string Status { get; }
        DateTime Time { get; }
    }
}