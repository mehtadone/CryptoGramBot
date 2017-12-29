namespace Jojatekok.PoloniexAPI.TradingTools
{
    public interface IOrder
    {
        ulong IdOrder { get; }

        OrderType Type { get; }

        double PricePerCoin { get; }
        double AmountQuote { get; }
        double AmountBase { get; }
    }
}
