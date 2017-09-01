using Newtonsoft.Json;
using System;

namespace Jojatekok.PoloniexAPI.TradingTools
{
    public class Trade : Order, ITrade
    {
        public string Pair { get; set; }

        public DateTime Time { get; private set; }

        [JsonProperty("date")]
        private string TimeInternal
        {
            set { Time = Helper.ParseDateTime(value); }
        }
    }
}