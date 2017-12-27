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

namespace CryptoGramBot.EventBus.Handlers.Bittrex
{
    public class BittrexDepositWithdrawalHandler : IEventHandler<DepositAndWithdrawalEvent>
    {
        private readonly BittrexService _bittrexService;
        private readonly IMicroBus _bus;
        private readonly BittrexConfig _config;
        private readonly DatabaseService _databaseService;
        private readonly GeneralConfig _generalConfig;
        private readonly PriceService _priceService;

        public BittrexDepositWithdrawalHandler(
            BittrexService bittrexService,
            BittrexConfig config,
            DatabaseService databaseService,
            PriceService priceService,
            GeneralConfig generalConfig,
            IMicroBus bus)
        {
            _bittrexService = bittrexService;
            _config = config;
            _databaseService = databaseService;
            _priceService = priceService;
            _generalConfig = generalConfig;
            _bus = bus;
        }

        public async Task Handle(DepositAndWithdrawalEvent @event)
        {
            if (_config.DepositNotification)
            {
                var deposits = await _bittrexService.GetNewDeposits();

                var i = 0;
                foreach (var deposit in deposits)
                {
                    if (i > 3)
                    {
                        var message = new StringBuilder($"{deposits.Count - i} deposits can be sent but not going as to avoid spamming");
                        await _bus.SendAsync(new SendMessageCommand(message));
                        break;
                    }

                    var priceInBtc = await _bittrexService.GetPrice(_generalConfig.TradingCurrency, deposit.Currency);
                    var btcAmount = priceInBtc * Convert.ToDecimal(deposit.Amount);
                    await SendDepositNotification(deposit, btcAmount);
                    i++;
                }
            }

            if (_config.WithdrawalNotification)
            {
                var withdrawals = await _bittrexService.GetNewWithdrawals();

                var i = 0;
                foreach (var withdrawal in withdrawals)
                {
                    if (i > 3)
                    {
                        var message = new StringBuilder($"{withdrawals.Count - i} withdrawals can be sent but not going as to avoid spamming");
                        await _bus.SendAsync(new SendMessageCommand(message));
                        break;
                    }

                    var priceInBtc = await _bittrexService.GetPrice(_generalConfig.TradingCurrency, withdrawal.Currency);
                    var btcAmount = priceInBtc * Convert.ToDecimal(withdrawal.Amount);
                    await SendWithdrawalNotification(withdrawal, btcAmount);
                    i++;
                }
            }
        }

        private async Task SendDepositNotification(Deposit deposit, decimal btcAmount)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{deposit.Time:g}");
            sb.AppendLine($"<strong>{Constants.Bittrex} Deposit of {deposit.Currency}</strong>");
            sb.AppendLine($"<strong>Currency: {deposit.Currency}</strong>");
            sb.AppendLine($"Amount: {deposit.Amount} ({btcAmount:##0.####} {_generalConfig.TradingCurrency})");

            await _bus.SendAsync(new SendMessageCommand(sb));
        }

        private async Task SendWithdrawalNotification(Withdrawal withdrawal, decimal btcAmount)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{withdrawal.Time:g}");
            sb.AppendLine($"<strong>{Constants.Bittrex} Withdrawal of {withdrawal.Currency}</strong>");
            sb.AppendLine($"Amount: {withdrawal.Amount} ({btcAmount:##0.####} {_generalConfig.TradingCurrency})");
            await _bus.SendAsync(new SendMessageCommand(sb));
        }
    }
}