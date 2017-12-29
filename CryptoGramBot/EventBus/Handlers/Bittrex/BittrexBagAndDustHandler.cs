using System;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using CryptoGramBot.Services;
using CryptoGramBot.Services.Data;
using CryptoGramBot.Services.Exchanges;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers.Bittrex
{
    public class BittrexBagAndDustHandler : IEventHandler<BagAndDustEvent>
    {
        private readonly BagConfig _bagConfig;
        private readonly BittrexService _bittrexService;
        private readonly IMicroBus _bus;
        private readonly DatabaseService _databaseService;
        private readonly DustConfig _dustConfig;
        private readonly GeneralConfig _generalConfig;
        private readonly LowBtcConfig _lowBtcConfig;

        public BittrexBagAndDustHandler(
            IMicroBus bus,
            GeneralConfig generalConfig,
            BittrexService bittrexService,
            DatabaseService databaseService,
            BagConfig bagConfig,
            DustConfig dustConfig,
            LowBtcConfig lowBtcConfig)
        {
            _bus = bus;
            _generalConfig = generalConfig;
            _bittrexService = bittrexService;
            _databaseService = databaseService;
            _bagConfig = bagConfig;
            _dustConfig = dustConfig;
            _lowBtcConfig = lowBtcConfig;
        }

        public async Task Handle(BagAndDustEvent @event)
        {
            var balanceInformation = await _bittrexService.GetBalance();

            foreach (var walletBalance in balanceInformation.WalletBalances)
            {
                if (walletBalance.Currency == Constants.BTC)
                {
                    if (_lowBtcConfig.Enabled)
                    {
                        if (walletBalance.BtcAmount <= _lowBtcConfig.LowBtcAmount)
                        {
                            await SendBtcLowNotification(walletBalance.BtcAmount);
                        }
                    }
                }

                if (walletBalance.Currency != Constants.BTC && walletBalance.Currency != "USDT" &&
                    walletBalance.Currency != "USD")
                {
                    var averagePrice = await _databaseService.GetBuyAveragePrice(_generalConfig.TradingCurrency, walletBalance.Currency, Constants.Poloniex, walletBalance.Available);
                    var currentPrice = await _bittrexService.GetPrice(_generalConfig.TradingCurrency, walletBalance.Currency);

                    if (_bagConfig.Enabled)
                    {
                        await BagManagement(currentPrice, averagePrice, walletBalance);
                    }

                    if (_dustConfig.Enabled)
                    {
                        await DustManagement(walletBalance);
                    }
                }
            }
        }

        private async Task BagManagement(decimal currentPrice, decimal averagePrice, WalletBalance walletBalance)
        {
            var percentageDrop = ProfitCalculator.PriceDifference(currentPrice, averagePrice);
            if (percentageDrop < -_bagConfig.PercentageDrop)
            {
                await SendBagNotification(walletBalance, averagePrice, currentPrice, percentageDrop);
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

        private async Task SendBagNotification(WalletBalance walletBalance, decimal averagePrice, decimal currentPrice, decimal percentageDrop)
        {
            var lastBought =
                await _databaseService.GetLastBoughtAsync(_generalConfig.TradingCurrency, walletBalance.Currency, Constants.Bittrex);

            var sb = new StringBuffer();
            sb.Append($"<strong>{Constants.Bittrex}</strong>: {DateTime.Now:g}\n");
            sb.Append($"<strong>Bag detected for {walletBalance.Currency}</strong>\n");
            sb.Append($"Average bought price: {averagePrice:#0.#############}\n");
            sb.Append($"Current price: {currentPrice:#0.#############}\n");
            sb.Append($"Percentage: {percentageDrop}%\n");
            sb.Append($"Bought on: {lastBought:g}\n");
            sb.Append($"Value: {walletBalance.Balance * currentPrice:#0.#####} {_generalConfig.TradingCurrency}");

            await _bus.SendAsync(new SendMessageCommand(sb));
        }

        private async Task SendBtcLowNotification(decimal walletBalanceBtcAmount)
        {
            var sb = new StringBuffer();
            sb.Append($"<strong>{Constants.Bittrex}</strong>: {DateTime.Now:g}\n");
            sb.Append($"<strong>Low {_generalConfig.TradingCurrency} detected</strong>\n");
            sb.Append($"{_generalConfig.TradingCurrency} Amount: {walletBalanceBtcAmount:#0.#############}");
            await _bus.SendAsync(new SendMessageCommand(sb));
        }

        private async Task SendDustNotification(WalletBalance walletBalance)
        {
            var sb = new StringBuffer();
            sb.Append($"<strong>{Constants.Bittrex}</strong>: {DateTime.Now:g}\n");
            sb.Append($"<strong>Dust detected for {walletBalance.Currency}</strong>\n");
            sb.Append($"{_generalConfig.TradingCurrency} Amount: {walletBalance.BtcAmount:#0.#############}\n");
            await _bus.SendAsync(new SendMessageCommand(sb));
        }
    }
}