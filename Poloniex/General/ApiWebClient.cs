using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Jojatekok.PoloniexAPI.TradingTools;
using Newtonsoft.Json.Linq;

namespace Jojatekok.PoloniexAPI
{
    internal sealed class ApiWebClient
    {
        public static readonly Encoding Encoding = Encoding.ASCII;
        private static readonly JsonSerializer JsonSerializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };
        private Authenticator _authenticator;
        private HMACSHA512 _encryptor = new HMACSHA512();

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

        public HMACSHA512 Encryptor
        {
            private get { return _encryptor; }
            set { _encryptor = value; }
        }

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