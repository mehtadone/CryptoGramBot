using System;
using System.Threading.Tasks;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.EventBus.Handlers;
using CryptoGramBot.EventBus.Handlers.Coinigy;
using CryptoGramBot.Helpers;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.Services.Telegram
{
    public class TelegramMessageSendingService
    {
        private readonly IMicroBus _bus;
        private readonly ILogger<TelegramMessageSendingService> _log;

        public TelegramMessageSendingService(
            ILogger<TelegramMessageSendingService> log,
            IMicroBus bus
        )
        {
            _log = log;
            _bus = bus;
        }

        public async Task BittrexBalance()
        {
            _log.LogInformation("24 Hour pnl difference for bittrex");
            await _bus.PublishAsync(new BalanceCheckEvent(true, Constants.Bittrex));
        }

        public async Task BittrexTradeImport()
        {
            BittrexFileUploadState.Waiting = true;
            var mess = new StringBuffer();
            mess.Append(StringContants.PleaseUploadBittrexFile);
            await _bus.SendAsync(new SendMessageCommand(mess));
        }

        public async Task CoinigyAccountBalance(string message)
        {
            _log.LogInformation("Message begins with /acc. Going to split string");
            var splitString = message.Split("_");

            try
            {
                var accountNumber = splitString[1];
                var account = int.Parse(accountNumber);

                _log.LogInformation($"PnL check for {account}");
                await _bus.PublishAsync(new BalanceCheckEvent(true, Constants.CoinigyAccountBalance, account));
            }
            catch (Exception)
            {
                await SendHelpMessage();
                _log.LogInformation($"Don't know what the user wants to do with the /acc. The message was {message}");
            }
        }

        public async Task CoinigyAccountList(string message)
        {
            try
            {
                _log.LogInformation("PnL Account List request");
                await _bus.SendAsync(new SendCoinigyAccountInfoCommand());
            }
            catch (Exception)
            {
                await SendHelpMessage();
                _log.LogInformation($"Don't know what the user wants to do with the /acc. The message was {message}");
            }
        }

        public async Task ExcelSheet()
        {
            _log.LogInformation("Excel sheet");
            await _bus.SendAsync(new ExcelExportCommand());
        }

        public async Task PoloniexBalance()
        {
            _log.LogInformation("24 Hour pnl difference for poloniex");
            await _bus.PublishAsync(new BalanceCheckEvent(true, Constants.Poloniex));
        }

        public async Task SendHelpMessage()
        {
            await _bus.SendAsync(new SendHelpMessageCommand());
        }

        public async Task TotalCoinigyBalance()
        {
            _log.LogInformation("24 Hour pnl difference for coinigy");
            await _bus.PublishAsync(new BalanceCheckEvent(true, Constants.TotalCoinigyBalance));
        }
    }
}