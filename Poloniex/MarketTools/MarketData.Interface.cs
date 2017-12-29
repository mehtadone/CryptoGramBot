namespace Jojatekok.PoloniexAPI.MarketTools
{
    public interface IMarketData
    {
        double PriceLast { get; }
        double PriceChangePercentage { get; }

        double Volume24HourBase { get; }
        double Volume24HourQuote { get; }

        double OrderTopBuy { get; }
        double OrderTopSell { get; }
        double OrderSpread { get; }
        double OrderSpreadPercentage { get; }

        bool IsFrozen { get; }
    }
}
