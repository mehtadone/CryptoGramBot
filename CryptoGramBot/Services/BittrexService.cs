using System;
using System.Collections.Generic;
using System.Globalization;
using Bittrex;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using Enexure.MicroBus;

namespace CryptoGramBot.Services
{
    public class BittrexService : IExchangeService
    {
        private readonly IMicroBus _bus;
        private readonly IExchange _exchange;

        public BittrexService(BittrexConfig config, IExchange exchange, IMicroBus bus)
        {
            _exchange = exchange;
            _bus = bus;
            var context = new ExchangeContext
            {
                QuoteCurrency = "BTC",
                Simulate = false,
                ApiKey = config.Key,
                Secret = config.Secret
            };

            exchange.Initialise(context);
        }

        public List<Trade> GetOrderHistory(DateTime lastChecked)
        {
            var response = _exchange.GetOrderHistory();
            var bittrexToTrades = TradeConverter.BittrexToTrades(response);
            return bittrexToTrades;
        }

        public decimal GetPrice(string terms)
        {
            var ticker = _exchange.GetTicker(terms);
            var price = ticker.Last.ToString();
            var priceAsDecimal = decimal.Parse(price, NumberStyles.Float);
            return priceAsDecimal;
        }

        public List<WalletBalance> GetWalletBalances()
        {
            var response = _exchange.GetBalances();
            var bittrexBalances = TradeConverter.BittrexToWalletBalances(response);
            return bittrexBalances;
        }
    }
}