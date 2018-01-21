using Binance.Api.WebSocket;
using Binance.Api.WebSocket.Events;
using Binance.Market;
using CryptoGramBot.Services.Exchanges.WebSockets.Binance.Events;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoGramBot.Services.Exchanges.WebSockets.Binance
{
    public class AllSymbolStatisticsWebSocketClient : SymbolStatisticsWebSocketClient
    {
        public event EventHandler<ManySymbolStatisticsEventArgs> ManyStatisticsUpdate;

        public AllSymbolStatisticsWebSocketClient(IWebSocketClient client, ILogger<SymbolStatisticsWebSocketClient> logger = null) : base(client, logger)
        {
        }

        public override Task SubscribeAsync(string symbol, Action<SymbolStatisticsEventArgs> callback, CancellationToken token)
        {
            return base.SubscribeAsync(symbol, callback, token);
        }

        public Task SubscribeAsync(Action<SymbolStatisticsEventArgs> callback, CancellationToken token)
        {
            return SubscribeToAsync($"/ws/!ticker@arr", callback, token);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        protected override void DeserializeJsonAndRaiseEvent(string json, CancellationToken token, Action<SymbolStatisticsEventArgs> callback = null)
        {
            if (IsJsonArray(json))
            {
                var eventTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                var statistics = JArray.Parse(json).Select(DeserializeSymbolStatistics).ToArray();

                ManyStatisticsUpdate?.Invoke(this, new ManySymbolStatisticsEventArgs(eventTime, token, statistics));
            }
            else
            {
                base.DeserializeJsonAndRaiseEvent(json, token, callback);
            }
        }

        #region Private Methods

        private static SymbolStatistics DeserializeSymbolStatistics(JToken jToken)
        {
            return new SymbolStatistics(
                jToken["s"].Value<string>(),  // symbol
                TimeSpan.FromHours(24),       // period
                jToken["p"].Value<decimal>(), // price change
                jToken["P"].Value<decimal>(), // price change percent
                jToken["w"].Value<decimal>(), // weighted average price
                jToken["x"].Value<decimal>(), // previous day's close price
                jToken["c"].Value<decimal>(), // current day's close price (last price)
                jToken["Q"].Value<decimal>(), // close trade's quantity (last quantity)
                jToken["b"].Value<decimal>(), // bid price
                jToken["B"].Value<decimal>(), // bid quantity
                jToken["a"].Value<decimal>(), // ask price
                jToken["A"].Value<decimal>(), // ask quantity
                jToken["o"].Value<decimal>(), // open price
                jToken["h"].Value<decimal>(), // high price
                jToken["l"].Value<decimal>(), // low price
                jToken["v"].Value<decimal>(), // base asset volume
                jToken["q"].Value<decimal>(), // quote asset volume
                jToken["O"].Value<long>(),    // open time
                jToken["C"].Value<long>(),    // close time
                jToken["F"].Value<long>(),    // first trade ID
                jToken["L"].Value<long>(),    // last trade ID
                jToken["n"].Value<long>());   // trade count
        }

        public static bool IsJsonArray(string s)
        {
            return !string.IsNullOrWhiteSpace(s)
                && s.StartsWith("[") && s.EndsWith("]");
        }

        #endregion Private Methods
    }
}
