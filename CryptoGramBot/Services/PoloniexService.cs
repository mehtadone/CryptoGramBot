using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using Jojatekok.PoloniexAPI;

namespace CryptoGramBot.Services
{
    public class PoloniexService : IExchangeService
    {
        private readonly PoloniexClient _poloniexClient;

        public PoloniexService(PoloniexConfig poloniexConfig)
        {
            _poloniexClient = new PoloniexClient(poloniexConfig.Key, poloniexConfig.Secret);
        }

        public List<Trade> GetOrderHistory(DateTime lastChecked)
        {
            var tradesAsync = _poloniexClient.Trading.GetTradesAsync(CurrencyPair.All, lastChecked);
            var tradesAsyncResult = tradesAsync.Result;

            var poloniexToTrades = TradeConverter.PoloniexToTrades(tradesAsyncResult);
            return poloniexToTrades;
        }

        public async Task<BalanceInformation> GetBalance(string accountName)
        {
            // TODO Polo Api is returning a bum result
            return null;
        }
    }
}