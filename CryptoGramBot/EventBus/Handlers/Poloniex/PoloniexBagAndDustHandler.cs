using System;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using CryptoGramBot.Services;
using CryptoGramBot.Services.Exchanges;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers.Poloniex
{
    public class PoloniexBagAndDustHandler : IEventHandler<BagAndDustEvent>
    {
        private readonly BagConfig _bagConfig;
        private readonly IMicroBus _bus;
        private readonly DatabaseService _databaseService;
        private readonly DustConfig _dustConfig;
        private readonly LowBtcConfig _lowBtcConfig;
        private readonly PoloniexService _poloService;

        public PoloniexBagAndDustHandler(IMicroBus bus, PoloniexService poloService, DatabaseService databaseService, BagConfig bagConfig, DustConfig dustConfig, LowBtcConfig lowBtcConfig)
        {
            _bus = bus;
            _poloService = poloService;
            _databaseService = databaseService;
            _bagConfig = bagConfig;
            _dustConfig = dustConfig;
            _lowBtcConfig = lowBtcConfig;
        }

        public async Task Handle(BagAndDustEvent @event)
        {
            var balanceInformation = await _poloService.GetBalance();

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

                if (walletBalance.Currency != "BTC" && walletBalance.Currency != "USDT" &&
                    walletBalance.Currency != "USD")
                {
                    var lastTradeForPair =
                        _databaseService.GetLastTradeForPair(walletBalance.Currency, Constants.Poloniex, TradeSide.Buy);
                    if (lastTradeForPair == null) continue;
                    var currentPrice = await _poloService.GetPrice(lastTradeForPair.Base, lastTradeForPair.Terms);

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
                $"<strong>{Constants.Poloniex}</strong>: {DateTime.Now:g}\n" +
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
                $"<strong>{Constants.Poloniex}</strong>: {DateTime.Now:g}\n" +
                $"<strong>Low BTC detected</strong>\n" +
                $"BTC Amount: {walletBalanceBtcAmount:#0.#############}\n";
            await _bus.SendAsync(new SendMessageCommand(message));
        }

        private async Task SendDustNotification(WalletBalance walletBalance)
        {
            var message =
                $"<strong>{Constants.Poloniex}</strong>: {DateTime.Now:g}\n" +
                $"<strong>Dust detected for {walletBalance.Currency}</strong>\n" +
                $"BTC Amount: {walletBalance.BtcAmount:#0.#############}\n";
            await _bus.SendAsync(new SendMessageCommand(message));
        }
    }
}