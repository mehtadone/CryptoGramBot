namespace Poloniex.WalletTools
{
    public interface IBalance
    {
        double BitcoinValue { get; }
        double QuoteAvailable { get; }
        double QuoteOnOrders { get; }
    }
}