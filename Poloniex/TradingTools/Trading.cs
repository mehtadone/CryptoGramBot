using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Jojatekok.PoloniexAPI;
using Newtonsoft.Json.Linq;
using Poloniex.General;
using IOrder = Poloniex.MarketTools.IOrder;
using ITrade = Poloniex.TradingTools.ITrade;
using Order = Poloniex.MarketTools.Order;
using Trade = Poloniex.MarketTools.Trade;

namespace Poloniex.TradingTools
{
    public class Trading : ITrading
    {
        internal Trading(ApiWebClient apiWebClient)
        {
            ApiWebClient = apiWebClient;
        }

        private ApiWebClient ApiWebClient { get; set; }

        public Task<bool> DeleteOrderAsync(CurrencyPair currencyPair, ulong orderId)
        {
            return Task.Factory.StartNew(() => DeleteOrder(currencyPair, orderId));
        }

        public Task<FeeInfo> GetFeeInfoAsync()
        {
            return Task.Factory.StartNew(GetFeeInfo);
        }

        public Task<IList<IOrder>> GetOpenOrdersAsync(CurrencyPair currencyPair)
        {
            return Task.Factory.StartNew(() => GetOpenOrders(currencyPair));
        }

        public Task<IList<ITrade>> GetTradesAsync(CurrencyPair currencyPair, DateTime startTime, DateTime endTime)
        {
            return Task.Factory.StartNew(() => GetTrades(currencyPair, startTime, endTime));
        }

        public Task<IList<ITrade>> GetTradesAsync(CurrencyPair currencyPair, DateTime startTime)
        {
            return Task.Factory.StartNew(() => GetTrades(currencyPair, startTime, DateTime.MaxValue));
        }

        public Task<IList<ITrade>> GetTradesAsync(CurrencyPair currencyPair)
        {
            return Task.Factory.StartNew(() => GetTrades(currencyPair, Helper.DateTimeUnixEpochStart, DateTime.MaxValue));
        }

        public Task<string> PostMarginOrderAsync(CurrencyPair currencyPair, OrderType type, double pricePerCoin, double amountQuote, double lendingRate)
        {
            return Task.Factory.StartNew(() => PostMarginOrder(currencyPair, type, pricePerCoin, amountQuote, lendingRate));
        }

        public Task<string> PostOrderAsync(CurrencyPair currencyPair, OrderType type, double pricePerCoin, double amountQuote)
        {
            return Task.Factory.StartNew(() => PostOrder(currencyPair, type, pricePerCoin, amountQuote));
        }

        private bool DeleteOrder(CurrencyPair currencyPair, ulong orderId)
        {
            var postData = new Dictionary<string, object> {
                { "currencyPair", currencyPair },
                { "orderNumber", orderId }
            };

            var data = PostData<JObject>("cancelOrder", postData);
            return data.Value<byte>("success") == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T GetData<T>(string command, params object[] parameters)
        {
            return ApiWebClient.GetData<T>(Helper.ApiUrlHttpsRelativePublic + command, parameters);
        }

        // TODO This is returning 0. DO NOT USE
        private FeeInfo GetFeeInfo()
        {
            var data = GetData<FeeInfo>(
                "returnFeeInfo");
            return data;
        }

        private IList<IOrder> GetOpenOrders(CurrencyPair currencyPair)
        {
            var postData = new Dictionary<string, object> {
                { "currencyPair", currencyPair }
            };

            var data = PostData<IList<Order>>("returnOpenOrders", postData);
            return data.Any() ? data.ToList<IOrder>() : new List<IOrder>();
        }

        private IList<ITrade> GetTrades(CurrencyPair currencyPair, DateTime startTime, DateTime endTime)
        {
            var postData = new Dictionary<string, object> {
                { "currencyPair", currencyPair },
                { "start", Helper.DateTimeToUnixTimeStamp(startTime) },
                { "end", Helper.DateTimeToUnixTimeStamp(endTime) }
            };

            if (currencyPair == CurrencyPair.All)
            {
                var allTrades = PostDataForAllTrades("returnTradeHistory", postData);
                return allTrades.Any() ? allTrades.ToList() : new List<ITrade>();
            }

            var data = PostData<IList<Trade>>("returnTradeHistory", postData);
            return data.Any() ? data.ToList<ITrade>() : new List<ITrade>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T PostData<T>(string command, Dictionary<string, object> postData)
        {
            return ApiWebClient.PostData<T>(command, postData);
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

            var data = PostData<JObject>(type.ToStringNormalized(), postData);
            if (data.Value<string>("error") != null)
                return data.Value<string>("error");

            return data.Value<string>("orderNumber");
        }

        private string PostOrder(CurrencyPair currencyPair, OrderType type, double pricePerCoin, double amountQuote)
        {
            var postData = new Dictionary<string, object> {
                { "currencyPair", currencyPair },
                { "rate", pricePerCoin.ToStringNormalized() },
                { "amount", amountQuote.ToStringNormalized() }
            };

            var data = PostData<JObject>(type.ToStringNormalized(), postData);
            if (data.Value<string>("error") != null)
                return data.Value<string>("error");

            return data.Value<string>("orderNumber");
        }
    }
}