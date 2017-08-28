using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json.Linq;
using Serilog.Core;
using TeleCoinigy.Configuration;
using TeleCoinigy.Database;
using TeleCoinigy.Models;

namespace TeleCoinigy.Services
{
    public class CoinigyApiService
    {
        private readonly Dictionary<int, Account> _coinigyAccounts = new Dictionary<int, Account>();
        private readonly CoinigyConfig _config;
        private readonly Logger _log;

        public CoinigyApiService(CoinigyConfig config, Logger log)
        {
            _config = config;
            _log = log;
        }

        public async Task<Dictionary<int, Account>> GetAccounts()
        {
            _log.Information($"Getting account list from Coinigy");
            if (_coinigyAccounts.Count == 0)
            {
                var jObject = await CommonApiQuery("accounts", "");
                var token = jObject["data"];

                int count = 1;
                foreach (var t in token)
                {
                    var account = new Account
                    {
                        AuthId = t["auth_id"].ToString(),
                        Name = t["auth_nickname"].ToString()
                    };

                    _coinigyAccounts[count] = account;
                    count++;
                }
            }
            return _coinigyAccounts;
        }

        public string GetAuthIdFor(string name)
        {
            _log.Information($"Getting authId for {name}");
            var singleOrDefault = _coinigyAccounts.Values.SingleOrDefault(x => x.Name == name);
            return singleOrDefault.AuthId;
        }

        public async Task<double> GetBtcBalance(string authId)
        {
            _log.Information($"Getting BTC balance for {authId}");
            var jObject = await CommonApiQuery("refreshBalance", "{  \"auth_id\":" + authId + "}");

            if (jObject != null)
            {
                var btcBalance = Helpers.Helpers.BalanceForAuthId(jObject);
                return Math.Round(btcBalance, 3);
            }
            return 0;
        }

        public async Task<double> GetBtcBalance()
        {
            _log.Information($"Getting total BTC balance");
            var jObject = await CommonApiQuery("balances", "{  \"show_nils\": 0,  \"auth_ids\": \"\"}");
            var btcBalance = Helpers.Helpers.TotalBtcBalance(jObject);
            return Math.Round(btcBalance, 3);
        }

        public async Task<double> GetTicker(string ticker)
        {
            _log.Information($"Getting ticker data for {ticker}");
            var jObject = await CommonApiQuery("ticker", "{  \"exchange_code\": \"GDAX\",  \"exchange_market\": \"" + ticker + "\"}");
            var bid = Helpers.Helpers.GetLastBid(jObject);
            return bid;
        }

        private async Task<JObject> CommonApiQuery(string apiCall, string stringContent)
        {
            var baseAddress = new Uri(_config.Endpoint);

            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-api-key", _config.Key);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-api-secret", _config.Secret);

                using (var content = new StringContent(stringContent, Encoding.Default, "application/json"))
                {
                    _log.Information($"Querying coinigy api: {baseAddress}/{apiCall} and content is {stringContent}");
                    using (var response = await httpClient.PostAsync(apiCall, content))
                    {
                        try
                        {
                            var responseData = await response.Content.ReadAsStringAsync();
                            return JObject.Parse(responseData);
                        }
                        catch (Exception exception)
                        {
                            var ex = exception.Message;
                            _log.Error(ex, "Exception when parsing response from Coinigy");
                            // coinigy sometimes returns an odd object here when trying refresh balance
                            return null;
                        }
                    }
                }
            }
        }
    }
}