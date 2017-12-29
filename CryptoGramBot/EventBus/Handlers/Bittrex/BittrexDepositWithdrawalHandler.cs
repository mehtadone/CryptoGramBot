using System;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using CryptoGramBot.Services.Exchanges;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers.Bittrex
{
    public class BittrexDepositWithdrawalHandler : IEventHandler<DepositAndWithdrawalEvent>
    {
        private readonly BittrexService _bittrexService;
        private readonly IMicroBus _bus;
        private readonly BittrexConfig _config;
        private readonly GeneralConfig _generalConfig;

        public BittrexDepositWithdrawalHandler(
            BittrexService bittrexService,
            BittrexConfig config,
            GeneralConfig generalConfig,
            IMicroBus bus)
        {
            _bittrexService = bittrexService;
            _config = config;
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
                    if (i > 30)
                    {
                        var message = new StringBuffer();
                        message.Append(StringContants.BittrexMoreThan30Deposits);
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
                    if (i > 30)
                    {
                        var message = new StringBuffer();
                        message.Append(StringContants.BittrexMoreThan30Deposits);
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
            var sb = new StringBuffer();
            sb.Append(string.Format("{0}", deposit.Time.ToString("g")));
            sb.Append(string.Format("<strong>{0} Deposit of {1}</strong>", Constants.Bittrex, deposit.Currency));
            sb.Append(string.Format("<strong>Currency: {0}</strong>", deposit.Currency));
            sb.Append(string.Format("Amount: {0} ({1} {2})", deposit.Amount, btcAmount.ToString("##0.####"), _generalConfig.TradingCurrency));

            await _bus.SendAsync(new SendMessageCommand(sb));
        }

        private async Task SendWithdrawalNotification(Withdrawal withdrawal, decimal btcAmount)
        {
            var sb = new StringBuffer();
            sb.Append(string.Format("{0}", withdrawal.Time.ToString("g")));
            sb.Append(string.Format("<strong>{0} Withdrawal of {1}</strong>", Constants.Bittrex, withdrawal.Currency));
            sb.Append(string.Format("Amount: {0} ({1} {2})", withdrawal.Amount, btcAmount.ToString("##0.####"), _generalConfig.TradingCurrency));
            await _bus.SendAsync(new SendMessageCommand(sb));
        }
    }
}