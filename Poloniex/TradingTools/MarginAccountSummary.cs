using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Jojatekok.PoloniexAPI.TradingTools
{
    public class MarginAccountSummary
        : IMarginAccountSummary
    {
        [JsonProperty("totalValue")]
        public double TotalValue { get; private set; }
        [JsonProperty("pl")]
        public double ProfitLoss { get; private set; }
        [JsonProperty("lendingFees")]
        public double LendingFees { get; private set; }
        [JsonProperty("netValue")]
        public double NetValue { get; private set; }
        [JsonProperty("totalBorrowedValue")]
        public double TotalBorrowedValue { get; private set; }
        [JsonProperty("currentMargin")]
        public double CurrentMargin { get; private set; }
    }
}
