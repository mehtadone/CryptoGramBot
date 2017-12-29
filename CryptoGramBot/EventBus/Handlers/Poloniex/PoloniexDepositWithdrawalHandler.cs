using System;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using CryptoGramBot.Services.Exchanges;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers.Poloniex
{
    public class PoloniexDepositWithdrawalHandler : IEventHandler<DepositAndWithdrawalEvent>
    {
        private readonly IMicroBus _bus;
        private readonly PoloniexConfig _config;
        private readonly GeneralConfig _generalConfig;
        private readonly PoloniexService _poloniexService;

        public PoloniexDepositWithdrawalHandler(
            PoloniexService poloniexService,
            PoloniexConfig config,
            GeneralConfig generalConfig,
            IMicroBus bus)
        {
            _poloniexService = poloniexService;
            _config = config;
            _generalConfig = generalConfig;
            _bus = bus;
        }

        public async Task Handle(DepositAndWithdrawalEvent @event)
        {
            if (_config.DepositNotification)
            {
                var deposits = await _poloniexService.GetNewDeposits();

                var i = 0;
                foreach (var deposit in deposits)
                {
                    if (i > 30)
                    {
                        var message = new StringBuffer();
                        message.Append(StringContants.PoloniexMoreThan30Deposits);
                        await _bus.SendAsync(new SendMessageCommand(message));
                        break;
                    }

                    var priceInBtc = await _poloniexService.GetPrice(_generalConfig.TradingCurrency, deposit.Currency);
                    var btcAmount = priceInBtc * Convert.ToDecimal(deposit.Amount);
                    await SendDepositNotification(deposit, btcAmount);
                    i++;
                }
            }

            if (_config.WithdrawalNotification)
            {
                var withdrawals = await _poloniexService.GetNewWithdrawals();

                var i = 0;
                foreach (var withdrawal in withdrawals)
                {
                    if (i > 3)
                    {
                        var message = new StringBuffer();
                        message.Append(StringContants.PoloniexMoreThan30Withdrawals);
                        break;
                    }

                    var priceInBtc = await _poloniexService.GetPrice(_generalConfig.TradingCurrency, withdrawal.Currency);
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
            sb.Append(string.Format("<strong>{0} Deposit of {1}</strong>", Constants.Poloniex, deposit.Currency));
            sb.Append(string.Format("<strong>Currency: {0}</strong>", deposit.Currency));
            sb.Append(string.Format("Amount: {0} ({1} {2})", deposit.Amount, btcAmount.ToString("##0.####"), _generalConfig.TradingCurrency));

            await _bus.SendAsync(new SendMessageCommand(sb));
        }

        private async Task SendWithdrawalNotification(Withdrawal withdrawal, decimal btcAmount)
        {
            var sb = new StringBuffer();
            sb.Append(string.Format("{0}", withdrawal.Time.ToString("g")));
            sb.Append(string.Format("<strong>{0} Withdrawal of {1}</strong>", Constants.Poloniex, withdrawal.Currency));
            sb.Append(string.Format("Amount: {0} ({1} {2})", withdrawal.Amount, btcAmount.ToString("##0.####"), _generalConfig.TradingCurrency));
            await _bus.SendAsync(new SendMessageCommand(sb));
        }
    }
}