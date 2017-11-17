using System;
using System.Threading.Tasks;
using BittrexSharp;

namespace CryptoGramBot.Services.Pricing
{
    public class PriceService
    {
        public async Task<decimal> GetDollarAmount(string baseCcy, decimal btcAmount)
        {
            var price = await GetPrice("USDT", baseCcy);
            return Math.Round(price * btcAmount, 2);
        }

        public async Task<decimal> GetPriceInBtc(string terms)
        {
            var apiKey = "...";
            var apiSecret = "...";
            var bittrex = new Bittrex(apiKey, apiSecret);

            var tcik = await bittrex.GetTicker("BTC", terms);

            if (tcik.Last.HasValue)
            {
                return tcik.Last.Value;
            }
            else
            {
                return 0;
            }
        }

        private async Task<decimal> GetPrice(string baseCcy, string termsCurrency)
        {
            var apiKey = "...";
            var apiSecret = "...";
            var bittrex = new Bittrex(apiKey, apiSecret);

            var tcik = await bittrex.GetTicker(baseCcy, termsCurrency);

            if (tcik.Last.HasValue)
            {
                return tcik.Last.Value;
            }
            else
            {
                return 0;
            }
        }
    }
}