namespace Jojatekok.PoloniexAPI.MarketTools
{
    /// <summary>Represents a time frame of a market.</summary>
    public enum MarketPeriod
    {
        /// <summary>A time interval of 5 minutes.</summary>
        Minutes5 = 300,

        /// <summary>A time interval of 15 minutes.</summary>
        Minutes15 = 900,

        /// <summary>A time interval of 30 minutes.</summary>
        Minutes30 = 1800,

        /// <summary>A time interval of 2 hours.</summary>
        Hours2 = 7200,

        /// <summary>A time interval of 4 hours.</summary>
        Hours4 = 14400,

        /// <summary>A time interval of a day.</summary>
        Day = 86400
    }
}
