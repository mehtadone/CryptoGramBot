using System;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.Database;
using CryptoGramBot.Models;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus
{
    public class BagManagementCommand : ICommand
    {
    }

    public class BagManagementHandler : ICommandHandler<BagManagementCommand>
    {
        private readonly BagConfig _bagConfig;
        private readonly BittrexService _bittrexService;
        private readonly IMicroBus _bus;
        private readonly DatabaseService _databaseService;

        public BagManagementHandler(IMicroBus bus, BittrexService bittrexService, DatabaseService databaseService, BagConfig bagConfig)
        {
            _bus = bus;
            _bittrexService = bittrexService;
            _databaseService = databaseService;
            _bagConfig = bagConfig;
        }

        public async Task Handle(BagManagementCommand command)
        {
            var walletBalances = _bittrexService.GetWalletBalances();

            foreach (var walletBalance in walletBalances)
            {
                if (walletBalance.Currency == "BTC") continue;

                var lastTradeForPair = _databaseService.GetLastTradeForPair(walletBalance.Currency);
                if (lastTradeForPair == null) continue;
                var currentPrice = _bittrexService.GetPrice(lastTradeForPair.Terms);

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