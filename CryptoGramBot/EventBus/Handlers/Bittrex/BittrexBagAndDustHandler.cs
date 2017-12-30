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
        private readonly BittrexService _bittrexService;
        private readonly IMicroBus _bus;
        private readonly BittrexConfig _config;
        private readonly DatabaseService _databaseService;
        private readonly GeneralConfig _generalConfig;

        public BittrexBagAndDustHandler(
            IMicroBus bus,
            GeneralConfig generalConfig,
            BittrexConfig config,
            BittrexService bittrexService,
            DatabaseService databaseService)
        {
            _bus = bus;
            _generalConfig = generalConfig;
            _config = config;
            _bittrexService = bittrexService;
            _databaseService = databaseService;
        }

        public async Task Handle(BagAndDustEvent @event)
        {
            var balanceInformation = await _bittrexService.GetBalance();

            foreach (var walletBalance in balanceInformation.WalletBalances)
            {
                if (walletBalance.Currency == Constants.BTC)
                {
                    if (_config.LowBtcNotification.HasValue)
                    {
                        if (walletBalance.BtcAmount <= _config.LowBtcNotification.Value)
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

                    if (_config.BagNotification.HasValue)
                    {
                        await BagManagement(currentPrice, averagePrice, walletBalance);
                    }

                    if (_config.DustNotification.HasValue)
                    {
                        await DustManagement(walletBalance);
                    }
                }
            }
        }

        private async Task BagManagement(decimal currentPrice, decimal averagePrice, WalletBalance walletBalance)
        {
            var percentageDrop = ProfitCalculator.PriceDifference(currentPrice, averagePrice);
            if (_config.BagNotification != null && percentageDrop < -_config.BagNotification.Value)
            {
                await SendBagNotification(walletBalance, averagePrice, currentPrice, percentageDrop);
            }
        }

        private async Task DustManagement(WalletBalance walletBalance)
        {
            var bagDetected = _config.DustNotification != null && walletBalance.BtcAmount <= _config.DustNotification.Value;
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
            sb.Append($"{StringContants.StrongOpen}{Constants.Bittrex}{StringContants.StrongClose}: {DateTime.Now:g}\n");
            sb.Append($"{StringContants.StrongOpen}Bag detected for {walletBalance.Currency}{StringContants.StrongClose}\n");
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
            sb.Append($"{StringContants.StrongOpen}{Constants.Bittrex}{StringContants.StrongClose}: {DateTime.Now:g}\n");
            sb.Append($"{StringContants.StrongOpen}Low {_generalConfig.TradingCurrency} detected{StringContants.StrongClose}\n");
            sb.Append($"{_generalConfig.TradingCurrency} Amount: {walletBalanceBtcAmount:#0.#############}");
            await _bus.SendAsync(new SendMessageCommand(sb));
        }

        private async Task SendDustNotification(WalletBalance walletBalance)
        {
            var sb = new StringBuffer();
            sb.Append($"{StringContants.StrongOpen}{Constants.Bittrex}{StringContants.StrongClose}: {DateTime.Now:g}\n");
            sb.Append($"{StringContants.StrongOpen}Dust detected for {walletBalance.Currency}{StringContants.StrongClose}\n");
            sb.Append($"{_generalConfig.TradingCurrency} Amount: {walletBalance.BtcAmount:#0.#############}\n");
            await _bus.SendAsync(new SendMessageCommand(sb));
        }
    }
}