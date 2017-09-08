using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.EventBus
{
    public class HelpMessageHandler : ICommandHandler<SendHelpMessageCommand>
    {
        private readonly BittrexConfig _bittrexConfig;
        private readonly IMicroBus _bus;
        private readonly CoinigyConfig _coinigyConfig;
        private readonly ILogger<HelpMessageHandler> _log;
        private readonly PoloniexConfig _poloniexConfig;

        public HelpMessageHandler(
            ILogger<HelpMessageHandler> log,
            BittrexConfig bittrexConfig,
            PoloniexConfig poloniexConfig,
            CoinigyConfig coinigyConfig,
            IMicroBus bus)
        {
            _log = log;
            _bittrexConfig = bittrexConfig;
            _poloniexConfig = poloniexConfig;
            _coinigyConfig = coinigyConfig;
            _bus = bus;
        }

        public async Task Handle(SendHelpMessageCommand command)
        {
            var usage = "<strong>Help</strong>\n\n" +
                "<strong>Common commands</strong>\n" +
                        $"{TelegramCommands.CommonExcel} - an excel export of all trades\n" +
                        $"{TelegramCommands.CommonPairProfit} - profit information for pair\n";

            if (_coinigyConfig.Enabled)
            {
                usage = usage + "\n<strong>Coinigy commands</strong>\n" +
                        $"{TelegramCommands.CoinigyAccountBalance} - 24 hour pnl for account number n.\n" +
                        $"{TelegramCommands.CoinigyAccountList} - all coinigy account names\n" +
                        $"{TelegramCommands.CoinigyTotalBalance} - 24 hour PnL for the total balance\n";
            }

            if (_bittrexConfig.Enabled)
            {
                usage = usage + "\n<strong>Bittrex commands</strong>\n" +
                        $"{TelegramCommands.BittrexTradeExportUpload} - upload bittrex order export";
            }

            _log.LogInformation("Sending help message");

            await _bus.SendAsync(new SendMessageCommand(usage));
        }
    }

    public class SendHelpMessageCommand : ICommand
    {
    }
}