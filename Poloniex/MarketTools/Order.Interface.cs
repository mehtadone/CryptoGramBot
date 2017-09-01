namespace Jojatekok.PoloniexAPI.MarketTools
{
    public interface IOrder
    {
        double PricePerCoin { get; }

        double AmountQuote { get; }
        double AmountBase { get; }
    }
}
