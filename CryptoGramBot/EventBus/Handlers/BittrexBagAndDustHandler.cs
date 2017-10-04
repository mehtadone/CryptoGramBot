using System;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers
{
    public class BittrexBagAndDustHandler : IEventHandler<BagAndDustEvent>
    {
        private readonly BagConfig _bagConfig;
        private readonly BittrexConfig _bittrexConfig;
        private readonly BittrexService _bittrexService;
        private readonly IMicroBus _bus;
        private readonly DatabaseService _databaseService;
        private readonly DustConfig _dustConfig;
        private readonly LowBtcConfig _lowBtcConfig;

        public BittrexBagAndDustHandler(IMicroBus bus, BittrexService bittrexService, DatabaseService databaseService, BagConfig bagConfig, BittrexConfig bittrexConfig, DustConfig dustConfig, LowBtcConfig lowBtcConfig)
        {
            _bus = bus;
            _bittrexService = bittrexService;
            _databaseService = databaseService;
            _bagConfig = bagConfig;
            _bittrexConfig = bittrexConfig;
            _dustConfig = dustConfig;
            _lowBtcConfig = lowBtcConfig;
        }

        public async Task Handle(BagAndDustEvent @event)
        {
            var balanceInformation = await _bittrexService.GetBalance();

            foreach (var walletBalance in balanceInformation.WalletBalances)
            {
                if (walletBalance.Currency == "BTC")
                {
                    if (_lowBtcConfig.Enabled)
                    {
                        if (walletBalance.BtcAmount <= _lowBtcConfig.LowBtcAmount)
                        {
                            await SendBtcLowNotification(walletBalance.BtcAmount);
                        }
                    }
                }

                var lastTradeForPair = _databaseService.GetLastTradeForPair(walletBalance.Currency, Constants.Bittrex, TradeSide.Buy);
                if (lastTradeForPair == null) continue;
                var currentPrice = await _bittrexService.GetPrice(lastTradeForPair.Terms);

                if (_bagConfig.Enabled)
                {
                    await BagManagement(currentPrice, lastTradeForPair, walletBalance);
                }

                if (_dustConfig.Enabled)
                {
                    await DustManagement(walletBalance);
                }
            }
        }

        private async Task BagManagement(decimal currentPrice, Trade lastTradeForPair, WalletBalance walletBalance)
        {
            var percentageDrop = ProfitCalculator.PriceDifference(currentPrice, lastTradeForPair.Limit);
            if (percentageDrop < -_bagConfig.PercentageDrop)
            {
                await SendBagNotification(walletBalance, lastTradeForPair, currentPrice, percentageDrop);
            }
        }

        private async Task DustManagement(WalletBalance walletBalance)
        {
            var bagDetected = walletBalance.BtcAmount <= _dustConfig.BtcAmount;
            if (bagDetected)
            {
                await SendDustNotification(walletBalance);
            }
        }

        private async Task SendBagNotification(WalletBalance walletBalance, Trade lastTradeForPair, decimal currentPrice, decimal percentageDrop)
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

        private async Task SendBtcLowNotification(decimal walletBalanceBtcAmount)
        {
            var message =
                $"<strong>{Constants.Bittrex}</strong>: {DateTime.Now:g}\n" +
                $"<strong>Low BTC detected</strong>\n" +
                $"BTC Amount: {walletBalanceBtcAmount:#0.#############}\n";
            await _bus.SendAsync(new SendMessageCommand(message));
        }

        private async Task SendDustNotification(WalletBalance walletBalance)
        {
            var message =
                $"<strong>{Constants.Bittrex}</strong>: {DateTime.Now:g}\n" +
                $"<strong>Dust detected for {walletBalance.Currency}</strong>\n" +
                $"BTC Amount: {walletBalance.BtcAmount:#0.#############}\n";
            await _bus.SendAsync(new SendMessageCommand(message));
        }
    }
}