using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TeleCoinigy.Configuration;
using TeleCoinigy.Database;
using TeleCoinigy.Models;

namespace TeleCoinigy.Services
{
    public class CoinigyApiService
    {
        private readonly List<Account> _coinigyAccounts = new List<Account>();
        private readonly CoinigyConfig _config;

        public CoinigyApiService(CoinigyConfig config)
        {
            _config = config;
        }

        public async Task<List<Account>> GetAccounts()
        {
            var jObject = await CommonApiQuery("accounts", "");
            var token = jObject["data"];

            foreach (var t in token)
            {
                var account = new Account
                {
                    AuthId = t["auth_id"].ToString(),
                    Name = t["auth_nickname"].ToString()
                };

                _coinigyAccounts.Add(account);
            }
            return _coinigyAccounts;
        }

        public string GetAuthIdFor(string name)
        {
            var singleOrDefault = _coinigyAccounts.SingleOrDefault(x => x.Name == name);
            return singleOrDefault.AuthId;
        }

        public async Task<double> GetBtcBalance(string authId)
        {
            var jObject = await CommonApiQuery("balances", "{  \"show_nils\": 0,  \"auth_ids\":" + authId + "}");
            var btcBalance = Helpers.Helpers.TotalBtcBalance(jObject);
            return Math.Round(btcBalance, 3);
        }

        public async Task<double> GetBtcBalance()
        {
            var jObject = await CommonApiQuery("balances", "{  \"show_nils\": 0,  \"auth_ids\": \"\"}");
            var btcBalance = Helpers.Helpers.TotalBtcBalance(jObject);
            return Math.Round(btcBalance, 3);
        }

        public async Task SaveBalancesForEachAccount(DatabaseService db)
        {
            foreach (var coinigyAccount in _coinigyAccounts)
            {
                var balance = await this.GetBtcBalance(coinigyAccount.AuthId);
                db.AddBalance(balance, coinigyAccount.Name);
            }
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
                    using (var response = await httpClient.PostAsync(apiCall, content))
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        return JObject.Parse(responseData);
                    }
                }
            }
        }
    }
}