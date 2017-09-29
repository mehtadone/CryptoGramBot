using System;
using Newtonsoft.Json;

namespace Poloniex.WalletTools
{
    public class Deposit : IDeposit
    {
        [JsonProperty("address")]
        public string Address { get; private set; }

        [JsonProperty("amount")]
        public double Amount { get; private set; }

        [JsonProperty("confirmations")]
        public uint Confirmations { get; private set; }

        [JsonProperty("currency")]
        public string Currency { get; private set; }

        [JsonProperty("status")]
        public string Status { get; private set; }

        public DateTime Time { get; private set; }

        [JsonProperty("txid")]
        public string TransactionId { get; private set; }

        [JsonProperty("timestamp")]
        private ulong TimeInternal
        {
            set { Time = Helper.UnixTimeStampToDateTime(value); }
        }
    }
}