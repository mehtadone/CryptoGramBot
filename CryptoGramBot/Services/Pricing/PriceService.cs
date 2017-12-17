using System;
using System.Threading.Tasks;
using CryptoGramBot.Helpers;
using CryptoGramBot.Services.Exchanges;

namespace CryptoGramBot.Services.Pricing
{
    public class PriceService
    {
        private readonly BittrexService _bittrexService;
        private readonly PoloniexService _poloniexService;

        public PriceService(BittrexService bittrexService, PoloniexService poloniexService)
        {
            _bittrexService = bittrexService;
            _poloniexService = poloniexService;
        }

        public async Task<decimal> GetDollarAmount(string baseCcy, decimal btcAmount, string exchange)
        {
            decimal price = 0;

            switch (exchange)
            {
                case Constants.Bittrex:
                    price = await _bittrexService.GetDollarAmount(baseCcy, btcAmount);
                    break;

                case Constants.Poloniex:
                    price = await _poloniexService.GetDollarAmount(baseCcy, btcAmount);
                    break;
            }

            return price;
        }

        public async Task<decimal> GetPrice(string baseCcy, string termsCurrency, string exchange)
        {
            decimal price = 0;

            switch (exchange)
            {
                case Constants.Bittrex:
                    price = await _bittrexService.GetPrice(baseCcy, termsCurrency);
                    break;

                case Constants.Poloniex:
                    price = await _poloniexService.GetPrice(baseCcy, termsCurrency);
                    break;
            }

            return price;
        }
    }
}