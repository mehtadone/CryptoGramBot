using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Bittrex.Data;

namespace Bittrex
{
    public class Exchange : IExchange
    {
        private const string ApiCallBuyLimit = "market/buylimit";
        private const string ApiCallCancel = "market/cancel";
        private const string ApiCallGetBalance = "account/getbalance";
        private const string ApiCallGetBalances = "account/getbalances";
        private const string ApiCallGetDeposits = "account/getdeposithistory";
        private const string ApiCallGetMarketHistory = "public/getmarkethistory";
        private const string ApiCallGetMarkets = "public/getmarkets";
        private const string ApiCallGetMarketSummary = "public/getmarketsummary";
        private const string ApiCallGetOpenOrders = "market/getopenorders";
        private const string ApiCallGetOrderBook = "public/getorderbook";
        private const string ApiCallGetOrderHistory = "account/getorderhistory";
        private const string ApiCallGetTicker = "public/getticker";
        private const string ApiCallGetWithdrawals = "account/getwithdrawalhistory";
        private const string ApiCallSellLimit = "market/selllimit";
        private const string ApiCallTemplate = "https://bittrex.com/api/{0}/{1}";
        private const string ApiVersion = "v1.1";
        private ApiCall _apiCall;
        private string _apiKey;
        private string _quoteCurrency;
        private string _secret;
        private bool _simulate;

        public decimal CalculateMinimumOrderQuantity(string market, decimal price)
        {
            var minimumQuantity = Math.Round(0.00050000M / price, 1) + 0.1M;
            return minimumQuantity;
        }

        public async Task CancelOrder(string uuid)
        {
            await Call<dynamic>(ApiCallCancel, Tuple.Create("uuid", uuid));
        }

        public async Task<AccountBalance> GetBalance(string market)
        {
            return await Call<AccountBalance>(ApiCallGetBalance, Tuple.Create("currency", market));
        }

        public async Task<GetBalancesResponse> GetBalances()
        {
            return await Call<GetBalancesResponse>(ApiCallGetBalances);
        }

        public async Task<GetDepositResponse> GetDeposits()
        {
            return await Call<GetDepositResponse>(ApiCallGetDeposits);
        }

        public async Task<GetMarketHistoryResponse> GetMarketHistory(string market, int count = 20)
        {
            return await Call<GetMarketHistoryResponse>(ApiCallGetMarketHistory,
                Tuple.Create("market", GetMarketName(market)),
                Tuple.Create("count", count.ToString()));
        }

        public dynamic GetMarkets()
        {
            return Call<dynamic>(ApiCallGetMarkets);
        }

        public async Task<GetMarketSummaryResponse> GetMarketSummary(string market)
        {
            var summaries = await Call<GetMarketSummaryResponse[]>(ApiCallGetMarketSummary,
                Tuple.Create("market", GetMarketName(market)));
            return summaries.FirstOrDefault();
        }

        public async Task<GetOpenOrdersResponse> GetOpenOrders(string market)
        {
            return await Call<GetOpenOrdersResponse>(ApiCallGetOpenOrders, Tuple.Create("market", GetMarketName(market)));
        }

        public async Task<GetOrderBookResponse> GetOrderBook(string market, OrderBookType type, int depth = 20)
        {
            if (type == OrderBookType.Both)
            {
                return await Call<GetOrderBookResponse>(ApiCallGetOrderBook,
                    Tuple.Create("market", GetMarketName(market)),
                    Tuple.Create("type", type.ToString().ToLower()),
                    Tuple.Create("depth", depth.ToString()));
            }
            else
            {
                var results = await Call<List<OrderEntry>>(ApiCallGetOrderBook,
                    Tuple.Create("market", GetMarketName(market)),
                    Tuple.Create("type", type.ToString().ToLower()),
                    Tuple.Create("depth", depth.ToString()));

                if (type == OrderBookType.Buy)
                {
                    return new GetOrderBookResponse { buy = results };
                }
                else
                {
                    return new GetOrderBookResponse { sell = results };
                }
            }
        }

        public async Task<GetOrderHistoryResponse> GetOrderHistory()
        {
            return await Call<GetOrderHistoryResponse>(ApiCallGetOrderHistory);
        }

        public async Task<GetOrderHistoryResponse> GetOrderHistory(string market, int count = 10)
        {
            return await Call<GetOrderHistoryResponse>(ApiCallGetOrderHistory,
                Tuple.Create("market", GetMarketName(market)),
                Tuple.Create("count", count.ToString()));
        }

        public dynamic GetTicker(string market)
        {
            return Call<dynamic>(ApiCallGetTicker, Tuple.Create("market", GetMarketName(market)));
        }

        public async Task<GetWithdrawalResponse> GetWithdrawals()
        {
            return await Call<GetWithdrawalResponse>(ApiCallGetWithdrawals);
        }

        public void Initialise(ExchangeContext context)
        {
            _apiKey = context.ApiKey;
            _secret = context.Secret;
            _quoteCurrency = context.QuoteCurrency;
            _simulate = context.Simulate;
            _apiCall = new ApiCall(_simulate);
        }

        public async Task<OrderResponse> PlaceBuyOrder(string market, decimal quantity, decimal price)
        {
            return await Call<OrderResponse>(ApiCallBuyLimit, Tuple.Create("market", GetMarketName(market)), Tuple.Create("quantity", quantity.ToString(CultureInfo.InvariantCulture)), Tuple.Create("rate", price.ToString(CultureInfo.InvariantCulture)));
        }

        public async Task<OrderResponse> PlaceSellOrder(string market, decimal quantity, decimal price)
        {
            return await Call<OrderResponse>(ApiCallSellLimit, Tuple.Create("market", GetMarketName(market)), Tuple.Create("quantity", quantity.ToString(CultureInfo.InvariantCulture)), Tuple.Create("rate", price.ToString(CultureInfo.InvariantCulture)));
        }

        private static string HashHmac(string message, string secret)
        {
            var encoding = Encoding.UTF8;
            using (var hmac = new HMACSHA512(encoding.GetBytes(secret)))
            {
                var msg = encoding.GetBytes(message);
                var hash = hmac.ComputeHash(msg);
                return BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);
            }
        }

        private async Task<T> Call<T>(string method, params Tuple<string, string>[] parameters)
        {
            if (method.StartsWith("public"))
            {
                var uri = string.Format(ApiCallTemplate, ApiVersion, method);
                if (parameters != null && parameters.Length > 0)
                {
                    var extraParameters = new StringBuilder();
                    foreach (var item in parameters)
                    {
                        extraParameters.Append((extraParameters.Length == 0 ? "?" : "&") + item.Item1 + "=" + item.Item2);
                    }

                    if (extraParameters.Length > 0)
                    {
                        uri = uri + extraParameters;
                    }
                }

                return await _apiCall.CallWithJsonResponse<T>(uri, false);
            }
            else
            {
                var nonce = DateTime.Now.Ticks;
                var uri = string.Format(ApiCallTemplate, ApiVersion, method + "?apikey=" + _apiKey + "&nonce=" + nonce);

                if (parameters != null)
                {
                    var extraParameters = new StringBuilder();
                    foreach (var item in parameters)
                    {
                        extraParameters.Append("&" + item.Item1 + "=" + item.Item2);
                    }

                    if (extraParameters.Length > 0)
                    {
                        uri = uri + extraParameters;
                    }
                }

                var sign = HashHmac(uri, _secret);
                return await _apiCall.CallWithJsonResponse<T>(uri,
                    !method.StartsWith("market/get") && !method.StartsWith("account/get"),
                    Tuple.Create("apisign", sign));
            }
        }

        private string GetMarketName(string market)
        {
            return _quoteCurrency + "-" + market;
        }
    }
}