using System;

namespace Poloniex.WalletTools
{
    public interface IDeposit
    {
        string Address { get; }
        double Amount { get; }
        uint Confirmations { get; }
        string Currency { get; }
        string Status { get; }
        DateTime Time { get; }
        string TransactionId { get; }
    }
}