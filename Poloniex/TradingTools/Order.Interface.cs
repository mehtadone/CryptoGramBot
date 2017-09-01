namespace Jojatekok.PoloniexAPI.TradingTools
{
    public interface IOrder
    {
        double AmountBase { get; }
        double AmountQuote { get; }
        ulong IdOrder { get; }

        double PricePerCoin { get; }
        OrderType Type { get; }
    }
}