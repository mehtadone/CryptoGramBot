using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CryptoGramBot.Services
{
    public class PriceService
    {
        private readonly DatabaseService _databaseService;
        private DateTime _lastChecked = DateTime.MinValue;
        private decimal _price;

        public PriceService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<decimal> GetDollarAmount(decimal btcAmount)
        {
            var price = await GetPriceFromCyptoApi("BTC", "USD");
            return Math.Round(price * btcAmount, 2);
        }

        public async Task<decimal> GetPriceFromCyptoApi(string ccy1, string ccy2)
        {
            if (_lastChecked > DateTime.Now - TimeSpan.FromMinutes(5))
            {
                return _price;
            }

            string url = "https://min-api.cryptocompare.com/data/price?fsym=BTC&tsyms=USD,EUR";
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(url))
                {
                    if (response.StatusCode != HttpStatusCode.OK) return _price;

                    var json = await response.Content.ReadAsStringAsync();
                    var jObject = JObject.Parse(json);
                    var stringPrice = jObject["USD"].ToString();
                    _price = decimal.Parse(stringPrice);
                    _lastChecked = DateTime.Now;
                }
            }

            return _price;
        }

        public async Task<decimal> GetPriceInBtc(string terms)
        {
            string url = $"https://min-api.cryptocompare.com/data/price?fsym={terms}&tsyms=BTC";
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(url))
                {
                    if (response.StatusCode != HttpStatusCode.OK) return _price;

                    var json = await response.Content.ReadAsStringAsync();
                    var jObject = JObject.Parse(json);
                    var stringPrice = jObject["BTC"].ToString();
                    _price = decimal.Parse(stringPrice, NumberStyles.Float);
                    _lastChecked = DateTime.Now;
                }
            }

            return _price;
        }
    }
}