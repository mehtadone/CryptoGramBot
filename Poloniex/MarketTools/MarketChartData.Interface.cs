using System;

namespace Poloniex.MarketTools
{
    public interface IMarketChartData
    {
        double Close { get; }
        double High { get; }
        double Low { get; }
        double Open { get; }
        DateTime Time { get; }
        double VolumeBase { get; }
        double VolumeQuote { get; }

        double WeightedAverage { get; }
    }
}