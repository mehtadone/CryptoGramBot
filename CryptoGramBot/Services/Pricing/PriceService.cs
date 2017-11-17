using System;
using System.Threading.Tasks;
using BittrexSharp;
using BittrexSharp.Domain;
using SQLitePCL;

namespace CryptoGramBot.Services.Pricing
{
    public class PriceService
    {
        public async Task<decimal> GetDollarAmount(string baseCcy, decimal btcAmount)
        {
            if (baseCcy == "USDT")
            {
                return Math.Round(btcAmount, 2);
            }

            var price = await GetPrice("USDT", baseCcy);
            return Math.Round(price * btcAmount, 2);
        }

        public async Task<decimal> GetPrice(string baseCcy, string termsCurrency)
        {
            var apiKey = "...";
            var apiSecret = "...";
            var bittrex = new Bittrex(apiKey, apiSecret);

            Ticker tcik = null;
            try
            {
                tcik = await bittrex.GetTicker(baseCcy, termsCurrency);
            }
            catch (Exception ex)
            {
                // should log
            }

            if (tcik != null && tcik.Last.HasValue)
            {
                return tcik.Last.Value;
            }

            var btcPrice = await bittrex.GetTicker("BTC", termsCurrency);

            if (btcPrice?.Last != null)
            {
                var btcBasePrice = await bittrex.GetTicker("BTC", baseCcy);
                if (btcBasePrice?.Last != null)
                {
                    return btcPrice.Last.Value * btcBasePrice.Last.Value;
                }
                else
                {
                    var baseBtcPrice = await bittrex.GetTicker(baseCcy, "BTC");

                    if (baseBtcPrice?.Last != null)
                    {
                        return baseBtcPrice.Last.Value * btcPrice.Last.Value;
                    }
                }
            }
            return 0;
        }
    }
}