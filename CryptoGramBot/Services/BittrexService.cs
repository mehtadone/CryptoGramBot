using System;
using System.Collections.Generic;
using Bittrex;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;

namespace CryptoGramBot.Services
{
    public class BittrexService : IExchangeService
    {
        private readonly IExchange _exchange;

        public BittrexService(BittrexConfig config, IExchange exchange)
        {
            _exchange = exchange;
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
    }
}