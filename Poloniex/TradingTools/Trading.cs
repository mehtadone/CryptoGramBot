using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Jojatekok.PoloniexAPI.Exceptions;
using Jojatekok.PoloniexAPI.MarketTools;

namespace Jojatekok.PoloniexAPI.TradingTools
{
    public class Trading : ITrading
    {
        internal Trading(ApiWebClient apiWebClient)
        {
            ApiWebClient = apiWebClient;
        }

        private ApiWebClient ApiWebClient { get; set; }

        public Task<IList<ITrade>> CloseMarginPositionAsync(CurrencyPair currencyPair)
        {
            return Task.Factory.StartNew(() => CloseMarginPosition(currencyPair));
        }

        public Task<bool> DeleteOrderAsync(CurrencyPair currencyPair, ulong orderId)
        {
            return Task.Factory.StartNew(() => DeleteOrder(currencyPair, orderId));
        }

        public Task<IDictionary<CurrencyPair, IPosition>> GetAllMarginPositionsAsync()
        {
            return Task.Factory.StartNew(GetAllMarginPositions);
        }

        public Task<IMarginAccountSummary> GetMarginAccountSummaryAsync()
        {
            return Task.Factory.StartNew(GetMarginAccountSummary);
        }

        public Task<IPosition> GetMarginPositionAsync(CurrencyPair currencyPair)
        {
            return Task.Factory.StartNew(() => GetMarginPosition(currencyPair));
        }

        public Task<IList<IOrder>> GetOpenOrdersAsync(CurrencyPair currencyPair)
        {
            return Task.Factory.StartNew(() => GetOpenOrders(currencyPair));
        }

        public Task<Dictionary<string, List<Order>>> GetOpenOrdersAsync()
        {
            return Task.Factory.StartNew(GetOpenOrders);
        }

        public Task<IDictionary<CurrencyPair, ITradeableBalance>> GetTradeableBalancesAsync()
        {
            return Task.Factory.StartNew(GetTradeableBalances);
        }

        public Task<IList<ITrade>> GetTradesAsync(CurrencyPair currencyPair, DateTime startTime, DateTime endTime)
        {
            return Task.Factory.StartNew(() => GetTrades(currencyPair, startTime, endTime));
        }

        public Task<IList<ITrade>> GetTradesAsync(CurrencyPair currencyPair)
        {
            return Task.Factory.StartNew(() => GetTrades(currencyPair, Helper.DateTimeUnixEpochStart, DateTime.MaxValue));
        }

        public Task<string> MoveOrderAsync(ulong orderId, double newRate, double? amount = null)
        {
            return Task.Factory.StartNew(() => MoveOrder(orderId, newRate, amount));
        }

        public Task<string> PostMarginOrderAsync(CurrencyPair currencyPair, OrderType type, double pricePerCoin, double amountQuote, double lendingRate)
        {
            return Task.Factory.StartNew(() => PostMarginOrder(currencyPair, type, pricePerCoin, amountQuote, lendingRate));
        }

        public Task<string> PostOrderAsync(CurrencyPair currencyPair, OrderType type, double pricePerCoin, double amountQuote)
        {
            return Task.Factory.StartNew(() => PostOrder(currencyPair, type, pricePerCoin, amountQuote));
        }

        private Dictionary<string, object> AddTradeFlags(Dictionary<string, object> target, TradeFlags flags)
        {
            if (flags.HasFlag(TradeFlags.ImmediateOrCancel)) { target["immediateOrCancel"] = 1; }
            if (flags.HasFlag(TradeFlags.PostOnly)) { target["postOnly"] = 1; }
            return target;
        }

        private IList<ITrade> CloseMarginPosition(CurrencyPair currencyPair)
        {
            var postData = new Dictionary<string, object> {
                { "currencyPair", currencyPair }
            };

            var trades = PostData<Dictionary<string, IList<Trade>>>("closeMarginPosition", postData, "resultingTrades");

            return trades != null && trades.Any()
                ? trades.SelectMany(x => x.Value).ToList<ITrade>()
                : new List<ITrade>();
        }

        private bool DeleteOrder(CurrencyPair currencyPair, ulong orderId)
        {
            var postData = new Dictionary<string, object> {
                { "currencyPair", currencyPair },
                { "orderNumber", orderId }
            };

            return PostData<byte>("cancelOrder", postData, "success", false) == 1;
        }

        private IDictionary<CurrencyPair, IPosition> GetAllMarginPositions()
        {
            var postData = new Dictionary<string, object> {
                { "currencyPair", "all" }
            };
            var data = PostData<IDictionary<string, Position>>("getMarginPosition", postData);
            return data
                    .Where(x => x.Value.Type != "none")
                    .ToDictionary(
                        x => CurrencyPair.Parse(x.Key),
                        x => (IPosition)x.Value);
        }

        private IMarginAccountSummary GetMarginAccountSummary()
        {
            var postData = new Dictionary<string, object>
            {
            };
            var data = PostData<MarginAccountSummary>("returnMarginAccountSummary", postData);
            return data;
        }

        private IPosition GetMarginPosition(CurrencyPair currencyPair)
        {
            var postData = new Dictionary<string, object> {
                { "currencyPair", currencyPair }
            };

            var data = PostData<Position>("getMarginPosition", postData);
            return data != null && data.Type != "none"
                ? data
                : default(IPosition);
        }

        private IList<IOrder> GetOpenOrders(CurrencyPair currencyPair)
        {
            var postData = new Dictionary<string, object>
                {
                    {"currencyPair", currencyPair}
                };

            var data = PostData<IList<Order>>("returnOpenOrders", postData);
            return data.Any() ? data.ToList<IOrder>() : new List<IOrder>();
        }

        private Dictionary<string, List<Order>> GetOpenOrders()
        {
            var postData = new Dictionary<string, object> {
                { "currencyPair", CurrencyPair.All }
            };

            var allOrders = PostDataForAllOrders("returnOpenOrders", postData);
            return allOrders;
        }

        private IDictionary<CurrencyPair, ITradeableBalance> GetTradeableBalances()
        {
            var postData = new Dictionary<string, object>
            {
            };

            var data = PostData<Dictionary<string, Dictionary<string, double>>>("returnTradableBalances", postData);

            return data.ToDictionary(x => CurrencyPair.Parse(x.Key), x => x.Value)
                .ToDictionary(x => x.Key, x => (ITradeableBalance)new TradeableBalance(x.Key, x.Value));
        }

        private IList<ITrade> GetTrades(CurrencyPair currencyPair, DateTime startTime, DateTime endTime)
        {
            var postData = new Dictionary<string, object> {
                { "currencyPair", currencyPair },
                { "start", Helper.DateTimeToUnixTimeStamp(startTime) },
                { "end", Helper.DateTimeToUnixTimeStamp(endTime) },
                { "limit", "10000"}
            };
            if (currencyPair == CurrencyPair.All)
            {
                var allTrades = PostDataForAllTrades("returnTradeHistory", postData);
                return allTrades.Any() ? allTrades.ToList() : new List<ITrade>();
            }
            var data = PostData<IList<Trade>>("returnTradeHistory", postData);
            return data.Any() ? data.ToList<ITrade>() : new List<ITrade>();
        }

        private string MoveOrder(ulong orderId, double newRate, double? amount = null, TradeFlags flags = TradeFlags.None)
        {
            var postData = new Dictionary<string, object> {
                { "orderNumber", orderId },
                { "rate", newRate }
            };
            if (amount.HasValue) { postData["amount"] = amount.Value; }
            postData = AddTradeFlags(postData, flags);

            return PostData<string>("moveOrder", postData, "orderNumber");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T PostData<T>(string command, Dictionary<string, object> postData, string fieldName = default(string), bool checkSuccess = true)
        {
            var ret = PostData(command, postData, checkSuccess);
            return fieldName == null
                ? ret.ToObject<T>()
                : ret.Value<T>(fieldName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private JObject PostData(string command, Dictionary<string, object> postData, bool checkSuccess = true)
        {
            var ret = ApiWebClient.PostData<JObject>(command, postData);
            return ThrowForFailure(ret, checkSuccess);
        }

        private Dictionary<string, List<Order>> PostDataForAllOrders(string command, Dictionary<string, object> postData)
        {
            return ApiWebClient.PostDataForAllOpenOrders(command, postData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private List<ITrade> PostDataForAllTrades(string command, Dictionary<string, object> postData)
        {
            return ApiWebClient.PostDataForAllTradeHistory(command, postData);
        }

        private string PostMarginOrder(CurrencyPair currencyPair, OrderType type, double pricePerCoin, double amountQuote, double lendingRate)
        {
            var postData = new Dictionary<string, object> {
                { "currencyPair", currencyPair },
                { "rate", pricePerCoin.ToStringNormalized() },
                { "amount", amountQuote.ToStringNormalized() },
                { "lendingRate", lendingRate.ToStringNormalized() }
            };

            return PostData<string>(type.ToStringNormalized(), postData, "orderNumber");
        }

        private string PostOrder(CurrencyPair currencyPair, OrderType type, double pricePerCoin, double amountQuote)
        {
            var postData = new Dictionary<string, object> {
                { "currencyPair", currencyPair },
                { "rate", pricePerCoin.ToStringNormalized() },
                { "amount", amountQuote.ToStringNormalized() }
            };

            return PostData<string>(type.ToStringNormalized(), postData, "orderNumber");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private JObject ThrowForFailure(JObject data, bool checkSuccess = true)
        {
            if (data.Value<string>("error") != null) { throw new TradeOperationFailureException(data); }
            if (checkSuccess)
            {
                JToken success;
                if (data.TryGetValue("success", out success))
                {
                    if (success.Value<byte>() != 1)
                    {
                        throw new TradeOperationFailureException(data);
                    }
                }
            }
            return data;
        }
    }
}