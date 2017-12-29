using Newtonsoft.Json;
using System;

namespace Jojatekok.PoloniexAPI.WalletTools
{
    public class Withdrawal : IWithdrawal
    {
        [JsonProperty("withdrawalNumber")]
        public ulong Id { get; private set; }

        [JsonProperty("currency")]
        public string Currency { get; private set; }
        [JsonProperty("address")]
        public string Address { get; private set; }
        [JsonProperty("amount")]
        public double Amount { get; private set; }

        [JsonProperty("timestamp")]
        private ulong TimeInternal {
            set { Time = Helper.UnixTimeStampToDateTime(value); }
        }
        public DateTime Time { get; private set; }
        [JsonProperty("ipAddress")]
        public string IpAddress { get; private set; }

        [JsonProperty("status")]
        public string Status { get; private set; }
    }
}
