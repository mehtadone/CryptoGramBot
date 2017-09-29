using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Poloniex.TradingTools
{
    public class FeeInfo
    {
        [JsonProperty("makerFee")]
        public decimal MakerFee { get; set; }

        [JsonProperty("nextTier")]
        public decimal NextTier { get; set; }

        [JsonProperty("takerFee")]
        public decimal TakerFee { get; set; }

        [JsonProperty("thirtyDayVolume")]
        public decimal ThirtyDayVolume { get; set; }
    }
}