using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.EventBus.Handlers
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
                        $"{TelegramCommands.CoinigyAccountList} - coinigy accounts and their balance\n" +
                        $"{TelegramCommands.CoinigyTotalBalance} - total balance from all acounts\n";
            }

            if (_bittrexConfig.Enabled)
            {
                usage = usage + "\n<strong>Bittrex commands</strong>\n" +
                        $"{TelegramCommands.BittrexTradeExportUpload} - upload bittrex order export\n" +
                        $"{TelegramCommands.BittrexBalanceInfo} - bittrex account summary\n";
            }
            if (_poloniexConfig.Enabled)
            {
                usage = usage + "\n<strong>Poloniex commands</strong>\n" +
                        $"{TelegramCommands.PoloniexBalanceInfo} - poloniex account summary\n" +
                        $"{TelegramCommands.PoloniexTradeReset} - reset trades database from poloniex";
            }

            _log.LogInformation("Sending help message");

            await _bus.SendAsync(new SendMessageCommand(usage));
        }
    }

    public class SendHelpMessageCommand : ICommand
    {
    }
}