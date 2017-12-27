using System;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using CryptoGramBot.Services;
using CryptoGramBot.Services.Exchanges;
using CryptoGramBot.Services.Pricing;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers.Poloniex
{
    public class PoloniexBagAndDustHandler : IEventHandler<BagAndDustEvent>
    {
        private readonly BagConfig _bagConfig;
        private readonly IMicroBus _bus;
        private readonly DatabaseService _databaseService;
        private readonly DustConfig _dustConfig;
        private readonly GeneralConfig _generalConfig;
        private readonly LowBtcConfig _lowBtcConfig;
        private readonly PoloniexService _poloService;
        private readonly PriceService _priceService;

        public PoloniexBagAndDustHandler(
            IMicroBus bus,
            PoloniexService poloService,
            DatabaseService databaseService,
            BagConfig bagConfig,
            DustConfig dustConfig,
            LowBtcConfig lowBtcConfig,
            PriceService priceService,
            GeneralConfig generalConfig)
        {
            _bus = bus;
            _poloService = poloService;
            _databaseService = databaseService;
            _bagConfig = bagConfig;
            _dustConfig = dustConfig;
            _lowBtcConfig = lowBtcConfig;
            _priceService = priceService;
            _generalConfig = generalConfig;
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
                    var averagePrice = await _databaseService.GetBuyAveragePrice(_generalConfig.TradingCurrency, walletBalance.Currency, Constants.Poloniex, walletBalance.Available);

                    var currentPrice = await _poloService.GetPrice(_generalConfig.TradingCurrency, walletBalance.Currency);

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
                await _databaseService.GetLastBoughtAsync(_generalConfig.TradingCurrency, walletBalance.Currency, Constants.Poloniex);

            var sb = new StringBuilder();
            sb.AppendLine($"<strong>{Constants.Poloniex}</strong>: {DateTime.Now:g}");
            sb.AppendLine($"<strong>Bag detected for {walletBalance.Currency}</strong>");
            sb.AppendLine($"Average bought price: {averagePrice:#0.#############}");
            sb.AppendLine($"Current price: {currentPrice:#0.#############}");
            sb.AppendLine($"Percentage: {percentageDrop}%");
            sb.AppendLine($"Bought on: {lastBought:g}");
            sb.AppendLine($"Value: {walletBalance.Balance * currentPrice:#0.#####} {_generalConfig.TradingCurrency}");

            await _bus.SendAsync(new SendMessageCommand(sb));
        }

        private async Task SendBtcLowNotification(decimal walletBalanceBtcAmount)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<strong>{Constants.Poloniex}</strong>: {DateTime.Now:g}");
            sb.AppendLine($"<strong>Low {_generalConfig.TradingCurrency} detected</strong>");
            sb.AppendLine($"{_generalConfig.TradingCurrency} Amount: {walletBalanceBtcAmount:#0.#############}");
            await _bus.SendAsync(new SendMessageCommand(sb));
        }

        private async Task SendDustNotification(WalletBalance walletBalance)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<strong>{Constants.Poloniex}</strong>: {DateTime.Now:g}");
            sb.AppendLine($"<strong>Dust detected for {walletBalance.Currency}</strong>");
            sb.AppendLine($"{_generalConfig.TradingCurrency} Amount: {walletBalance.BtcAmount:#0.#############}");
            await _bus.SendAsync(new SendMessageCommand(sb));
        }
    }
}