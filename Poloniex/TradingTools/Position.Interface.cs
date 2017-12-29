namespace Jojatekok.PoloniexAPI.TradingTools
{
    /// <summary>
    /// Describes a margin position
    /// </summary>
    public interface IPosition
    {
        /// <summary>
        /// Amount
        /// </summary>
        double Amount { get; }
        /// <summary>
        /// Total Value
        /// </summary>
        double Total { get; }
        /// <summary>
        /// Base price of the position
        /// </summary>
        double BasePrice { get; }
        /// <summary>
        /// Position liquidation price
        /// </summary>
        double LiquidationPrice { get; }
        /// <summary>
        /// Profit or loss of this position
        /// </summary>
        double ProfitLoss { get; }
        /// <summary>
        /// Amount due in lending fees
        /// </summary>
        double LendingFees { get; }
        /// <summary>
        /// Not sure
        /// </summary>
        string Type { get; }
    }
}
