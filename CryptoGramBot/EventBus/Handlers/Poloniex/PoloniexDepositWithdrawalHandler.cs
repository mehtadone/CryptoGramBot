using System;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers.Poloniex
{
    public class PoloniexDepositWithdrawalHandler : IEventHandler<DepositAndWithdrawalEvent>
    {
        private readonly IMicroBus _bus;
        private readonly PoloniexConfig _config;
        private readonly DatabaseService _databaseService;
        private readonly PoloniexService _poloniexService;
        private readonly PriceService _priceService;

        public PoloniexDepositWithdrawalHandler(
            PoloniexService poloniexService,
            PoloniexConfig config,
            DatabaseService databaseService,
            PriceService priceService,
            IMicroBus bus)
        {
            _poloniexService = poloniexService;
            _config = config;
            _databaseService = databaseService;
            _priceService = priceService;
            _bus = bus;
        }

        public async Task Handle(DepositAndWithdrawalEvent @event)
        {
            if (_config.DepositNotification)
            {
                var deposits = await _poloniexService.GetNewDeposits();
                await _databaseService.AddDeposits(deposits);

                var i = 0;
                foreach (var deposit in deposits)
                {
                    if (i > 5)
                    {
                        var message = $"{deposits.Count - i} withdrawals can be sent but not going to send more messages to avoid spamming";
                        await _bus.SendAsync(new SendMessageCommand(message));
                        break;
                    }

                    var priceInBtc = await _priceService.GetPriceInBtc(deposit.Currency);
                    var btcAmount = priceInBtc * Convert.ToDecimal(deposit.Amount);
                    await SendDepositNotification(deposit, btcAmount);
                    i++;
                }
            }

            if (_config.WithdrawalNotification)
            {
                var withdrawals = await _poloniexService.GetNewWithdrawals();
                await _databaseService.AddWithdrawals(withdrawals);

                var i = 0;
                foreach (var withdrawal in withdrawals)
                {
                    if (i > 5)
                    {
                        var message = $"{withdrawals.Count - i} withdrawals can be sent but not going to send more messages to avoid spamming";
                        await _bus.SendAsync(new SendMessageCommand(message));
                        break;
                    }

                    var priceInBtc = await _priceService.GetPriceInBtc(withdrawal.Currency);
                    var btcAmount = priceInBtc * Convert.ToDecimal(withdrawal.Amount);
                    await SendWithdrawalNotification(withdrawal, btcAmount);
                    i++;
                }
            }
        }

        private async Task SendDepositNotification(Deposit deposit, decimal btcAmount)
        {
            var message =
                $"{deposit.Time:g}\n" +
                $"<strong>{Constants.Poloniex} Deposit of {deposit.Currency}</strong>\n" +
                $"<strong>Currency: {deposit.Currency}</strong>\n" +
                $"Amount: {deposit.Amount} ({btcAmount} BTC)\n";
            await _bus.SendAsync(new SendMessageCommand(message));
        }

        private async Task SendWithdrawalNotification(Withdrawal withdrawal, decimal btcAmount)
        {
            var message =
                $"{withdrawal.Time:g}\n" +
                $"<strong>{Constants.Poloniex} Withdrawal of {withdrawal.Currency}</strong>\n" +
                $"Amount: {withdrawal.Amount} ({btcAmount} BTC)\n";
            await _bus.SendAsync(new SendMessageCommand(message));
        }
    }
}