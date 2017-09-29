using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using Jojatekok.PoloniexAPI;
using Poloniex;
using Poloniex.General;

namespace CryptoGramBot.Services
{
    public class PoloniexService : IExchangeService
    {
        private readonly PoloniexClient _poloniexClient;

        public PoloniexService(PoloniexConfig poloniexConfig)
        {
            _poloniexClient = new PoloniexClient(poloniexConfig.Key, poloniexConfig.Secret);
        }

        public async Task<BalanceInformation> GetBalance()
        {
            // TODO Polo Api is returning a bum result
            return null;
        }

        public async Task<List<Trade>> GetOrderHistory(DateTime lastChecked)
        {
            var tradesAsync = await _poloniexClient.Trading.GetTradesAsync(CurrencyPair.All, lastChecked);
            var tradesAsyncResult = tradesAsync;

            var feeInfo = await _poloniexClient.Trading.GetFeeInfoAsync();

            var poloniexToTrades = TradeConverter.PoloniexToTrades(tradesAsyncResult, feeInfo);
            return poloniexToTrades;
        }
    }
}