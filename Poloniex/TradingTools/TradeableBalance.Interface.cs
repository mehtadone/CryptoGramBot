
namespace Jojatekok.PoloniexAPI.TradingTools
{
    /// <summary>
    /// The <see cref="ITradeableBalance"/> interface is used when describing margin tradeable balance for a given market.
    /// </summary>
    public interface ITradeableBalance
    {
        /// <summary>
        /// The amount of currency available for a buy order.
        /// </summary>
        double Buy { get; }
        /// <summary>
        /// The amount of currency available for a sell order.
        /// </summary>
        double Sell { get; }
    }
}
