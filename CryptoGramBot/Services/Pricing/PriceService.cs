using System;
using System.Threading.Tasks;
using CryptoGramBot.Helpers;
using CryptoGramBot.Services.Exchanges;

namespace CryptoGramBot.Services.Pricing
{
    public class PriceService
    {
        private readonly BinanceService _binanceService;
        private readonly BittrexService _bittrexService;
        private readonly PoloniexService _poloniexService;

        public PriceService(BittrexService bittrexService, PoloniexService poloniexService, BinanceService binanceService)
        {
            _bittrexService = bittrexService;
            _poloniexService = poloniexService;
            _binanceService = binanceService;
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

                case Constants.Binance:
                    price = await _binanceService.GetDollarAmount(baseCcy, btcAmount);
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

                case Constants.Binance:
                    price = await _binanceService.GetPrice(baseCcy, termsCurrency);
                    break;
            }

            return price;
        }
    }
}