using System;

namespace Jojatekok.PoloniexAPI.WalletTools
{
    public interface IDeposit
    {
        string Currency { get; }
        string Address { get; }
        double Amount { get; }

        DateTime Time { get; }
        string TransactionId { get; }
        uint Confirmations { get; }

        string Status { get; }
    }
}
