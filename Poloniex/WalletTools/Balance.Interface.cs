namespace Jojatekok.PoloniexAPI.WalletTools
{
    public interface IBalance
    {
        double QuoteAvailable { get; }
        double QuoteOnOrders { get; }
        double BitcoinValue { get; }
    }
}
