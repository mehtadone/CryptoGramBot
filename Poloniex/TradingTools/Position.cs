using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Jojatekok.PoloniexAPI.TradingTools
{
    /// <summary>
    /// Describes a margin position
    /// </summary>
    /// <remarks>{"amount":"40.94717831","total":"-0.09671314",""basePrice":"0.00236190","liquidationPrice":-1,"pl":"-0.00058655", "lendingFees":"-0.00000038","type":"long"}</remarks>
    public class Position
        : IPosition
    {
        [JsonProperty("amount")]
        public double Amount { get; private set; }
        [JsonProperty("total")]
        public double Total { get; private set; }
        [JsonProperty("basePrice")]
        public double BasePrice { get; private set; }
        [JsonProperty("liquidationPrice")]
        public double LiquidationPrice { get; private set; }
        [JsonProperty("pl")]
        public double ProfitLoss { get; private set; }
        [JsonProperty("lendingFees")]
        public double LendingFees { get; private set; }
        [JsonProperty("type")]
        public string Type { get; private set; }
    }
}
