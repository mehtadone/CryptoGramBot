using System;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.Models;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers
{
    public class BittrexBagManagementHandler : IEventHandler<BagManagementEvent>
    {
        private readonly BagConfig _bagConfig;
        private readonly BittrexConfig _bittrexConfig;
        private readonly BittrexService _bittrexService;
        private readonly IMicroBus _bus;
        private readonly DatabaseService _databaseService;

        public BittrexBagManagementHandler(IMicroBus bus, BittrexService bittrexService, DatabaseService databaseService, BagConfig bagConfig, BittrexConfig bittrexConfig)
        {
            _bus = bus;
            _bittrexService = bittrexService;
            _databaseService = databaseService;
            _bagConfig = bagConfig;
            _bittrexConfig = bittrexConfig;
        }

        public async Task Handle(BagManagementEvent @event)
        {
            var balanceInformation = await _bittrexService.GetBalance(_bittrexConfig.Name);

            foreach (var walletBalance in balanceInformation.WalletBalances)
            {
                if (walletBalance.Currency == "BTC") continue;

                var lastTradeForPair = _databaseService.GetLastTradeForPair(walletBalance.Currency, _bittrexConfig.Name, TradeSide.Buy);
                if (lastTradeForPair == null) continue;
                var currentPrice = await _bittrexService.GetPrice(lastTradeForPair.Terms);

                if (_bagConfig.PercentageDrop > 30)
                {
                    await SendNotification(walletBalance, lastTradeForPair, currentPrice);
                }
            }
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