namespace Poloniex.MarketTools
{
    public interface IOrder
    {
        double AmountBase { get; }
        double AmountQuote { get; }
        double PricePerCoin { get; }
    }
}