using System;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
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
            var balanceInformation = await _bittrexService.GetBalance();

            foreach (var walletBalance in balanceInformation.WalletBalances)
            {
                if (walletBalance.Currency == "BTC") continue;

                var lastTradeForPair = _databaseService.GetLastTradeForPair(walletBalance.Currency, Constants.Bittrex, TradeSide.Buy);
                if (lastTradeForPair == null) continue;
                var currentPrice = await _bittrexService.GetPrice(lastTradeForPair.Terms);

                var percentageDrop = ProfitCalculator.PriceDifference(currentPrice, lastTradeForPair.Limit);
                if (_bagConfig.PercentageDrop < percentageDrop)
                {
                    await SendNotification(walletBalance, lastTradeForPair, currentPrice, percentageDrop);
                }
            }
        }

        private async Task SendNotification(WalletBalance walletBalance, Trade lastTradeForPair, decimal currentPrice, decimal percentageDrop)
        {
            var message =
                $"<strong>{Constants.Bittrex}</strong>: {DateTime.Now:g}\n" +
                $"<strong>Bag detected for {walletBalance.Currency}</strong>\n" +
                $"Bought price: {lastTradeForPair.Limit:#0.#############}\n" +
                $"Current price: {currentPrice:#0.#############}\n" +
                $"Percentage: {percentageDrop}%\n" +
                $"Bought on: {lastTradeForPair.TimeStamp:g}\n" +
                $"Value: {walletBalance.Balance * currentPrice:#0.#####} BTC";

            await _bus.SendAsync(new SendMessageCommand(message));
        }
    }
}