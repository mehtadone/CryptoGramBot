using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Models;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers
{
    public class PoloniexBagAndDustHandler : IEventHandler<BagAndDustEvent>
    {
        private readonly BagConfig _bagConfig;
        private readonly IMicroBus _bus;
        private readonly DatabaseService _databaseService;
        private readonly DustConfig _dustConfig;
        private readonly PoloniexService _poloService;

        public PoloniexBagAndDustHandler(IMicroBus bus, PoloniexService poloService, DatabaseService databaseService, BagConfig bagConfig, DustConfig dustConfig)
        {
            _bus = bus;
            _poloService = poloService;
            _databaseService = databaseService;
            _bagConfig = bagConfig;
            _dustConfig = dustConfig;
        }

        public async Task Handle(BagAndDustEvent @event)
        {
            //TODO Add bag management for polo
            //            var walletBalances = _bittrexService.GetWalletBalances();
            //
            //            foreach (var walletBalance in walletBalances)
            //            {
            //                if (walletBalance.Currency == "BTC") continue;
            //
            //                var lastTradeForPair = _databaseService.GetLastTradeForPair(walletBalance.Currency);
            //                if (lastTradeForPair == null) continue;
            //                var currentPrice = await _bittrexService.GetPrice(lastTradeForPair.Terms);
            //
            //                if (_bagConfig.PercentageChange > 30)
            //                {
            //                    await SendNotification(walletBalance, lastTradeForPair, currentPrice);
            //                }
            //            }
        }

        private static decimal PriceDifference(decimal currentPrice, decimal limit)
        {
            var percentage = (currentPrice - limit) / limit * 100;
            return Math.Round(percentage, 0);
        }

        private async Task SendNotification(WalletBalance walletBalance, Trade lastTradeForPair, decimal currentPrice)
        {
            var message =
                $"{DateTime.Now:g}\n" +
                $"<strong>Bag detected for {walletBalance.Currency}</strong>\n" +
                $"Bought price: {lastTradeForPair.Limit:#0.#############}\n" +
                $"Current price: {currentPrice:#0.#############}\n" +
                $"Percentage drop: {PriceDifference(currentPrice, lastTradeForPair.Limit)}%\n" +
                $"Bought on: {lastTradeForPair.TimeStamp:g}\n" +
                $"Value: {walletBalance.Balance * currentPrice:#0.#############}";

            await _bus.SendAsync(new SendMessageCommand(message));
        }
    }
}