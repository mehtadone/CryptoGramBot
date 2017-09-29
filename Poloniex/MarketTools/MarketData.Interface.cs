namespace Poloniex.MarketTools
{
    public interface IMarketData
    {
        bool IsFrozen { get; }
        double OrderSpread { get; }
        double OrderSpreadPercentage { get; }
        double OrderTopBuy { get; }
        double OrderTopSell { get; }
        double PriceChangePercentage { get; }
        double PriceLast { get; }
        double Volume24HourBase { get; }
        double Volume24HourQuote { get; }
    }
}