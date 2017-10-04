using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Poloniex.TradingTools;

namespace Poloniex.General
{
    internal sealed class ApiWebClient
    {
        public static readonly Encoding Encoding = Encoding.ASCII;
        private static readonly JsonSerializer JsonSerializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };
        private Authenticator _authenticator;

        public ApiWebClient(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        public Authenticator Authenticator
        {
            private get { return _authenticator; }

            set
            {
                _authenticator = value;
                Encryptor.Key = Encoding.GetBytes(value.PrivateKey);
            }
        }

        public string BaseUrl { get; private set; }

        public HMACSHA512 Encryptor { get; set; } = new HMACSHA512();

        public T GetData<T>(string command, params object[] parameters)
        {
            var relativeUrl = CreateRelativeUrl(command, parameters);

            var jsonString = QueryString(relativeUrl);
            var output = JsonSerializer.DeserializeObject<T>(jsonString);

            return output;
        }

        public T PostData<T>(string command, Dictionary<string, object> postData)
        {
            postData.Add("command", command);
            postData.Add("nonce", Helper.GetCurrentHttpPostNonce());

            var jsonString = PostString(Helper.ApiUrlHttpsRelativeTrading, postData.ToHttpPostString());
            var output = JsonSerializer.DeserializeObject<T>(jsonString);

            return output;
        }

        public Dictionary<string, IOrder> PostDataForAllOpenOrders(string command, Dictionary<string, object> postData)
        {
            postData.Add("command", command);
            postData.Add("nonce", Helper.GetCurrentHttpPostNonce());

            var jsonString = PostString(Helper.ApiUrlHttpsRelativeTrading, postData.ToHttpPostString());
            var list = new Dictionary<string, IOrder>();

            try
            {
                var output = JObject.Parse(jsonString);
                foreach (var token in output)
                {
                    if (!token.Value.HasValues) continue;

                    var pairTrades = JsonSerializer.DeserializeObject<List<Order>>(token.Value.ToString());

                    foreach (var pairTrade in pairTrades)
                    {
                        list.Add(token.Key, pairTrade);
                    }
                }
            }
            catch (JsonReaderException e)
            {
                var ex = e;
            }

            return list;
        }

        public List<ITrade> PostDataForAllTradeHistory(string command, Dictionary<string, object> postData)
        {
            postData.Add("command", command);
            postData.Add("nonce", Helper.GetCurrentHttpPostNonce());

            var jsonString = PostString(Helper.ApiUrlHttpsRelativeTrading, postData.ToHttpPostString());
            var list = new List<ITrade>();

            try
            {
                var output = JObject.Parse(jsonString);
                foreach (var token in output)
                {
                    var pairTrades = JsonSerializer.DeserializeObject<List<Trade>>(token.Value.ToString());

                    foreach (var pairTrade in pairTrades)
                    {
                        pairTrade.Pair = token.Key;
                    }

                    list.AddRange(pairTrades);
                }
            }
            catch (JsonReaderException e)
            {
                var ex = e;
            }

            return list;
        }

        private static string CreateRelativeUrl(string command, object[] parameters)
        {
            var relativeUrl = command;
            if (parameters.Length != 0)
            {
                relativeUrl += "&" + string.Join("&", parameters);
            }

            return relativeUrl;
        }

        private HttpWebRequest CreateHttpWebRequest(string method, string relativeUrl)
        {
            var request = WebRequest.CreateHttp(BaseUrl + relativeUrl);
            request.Method = method;
            request.UserAgent = "Poloniex API .NET v" + Helper.AssemblyVersionString;

            request.Timeout = Timeout.Infinite;

            request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip,deflate";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            return request;
        }

        private string PostString(string relativeUrl, string postData)
        {
            var request = CreateHttpWebRequest("POST", relativeUrl);
            request.ContentType = "application/x-www-form-urlencoded";

            var postBytes = Encoding.GetBytes(postData);
            request.ContentLength = postBytes.Length;

            request.Headers["Key"] = Authenticator.PublicKey;
            request.Headers["Sign"] = Encryptor.ComputeHash(postBytes).ToStringHex();

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(postBytes, 0, postBytes.Length);
            }

            return request.GetResponseString();
        }

        private string QueryString(string relativeUrl)
        {
            var request = CreateHttpWebRequest("GET", relativeUrl);

            return request.GetResponseString();
        }
    }
}