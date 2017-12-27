using System.Text;
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
            var sb = new StringBuilder();
            sb.AppendLine("<strong>Help</strong>");
            sb.AppendLine();
            sb.AppendLine("<strong>Common commands</strong>");
            sb.AppendLine($"{TelegramCommands.CommonExcel} - an excel export of all trades");
            sb.AppendLine($"{TelegramCommands.CommonPairProfit} - profit information for pair");

            if (_coinigyConfig.Enabled)
            {
                sb.AppendLine("\n<strong>Coinigy commands</strong>)");
                sb.AppendLine();
                sb.AppendLine($"{TelegramCommands.CoinigyAccountList} - coinigy accounts and their balance");
                sb.AppendLine($"{TelegramCommands.CoinigyTotalBalance} - total balance from all acounts");
            }

            if (_bittrexConfig.Enabled)
            {
                sb.AppendLine("<strong>Bittrex commands</strong>");
                sb.AppendLine();
                sb.AppendLine($"{TelegramCommands.BittrexTradeExportUpload} - upload bittrex order export");
                sb.AppendLine($"{TelegramCommands.BittrexBalanceInfo} - bittrex account summary");
            }
            if (_poloniexConfig.Enabled)
            {
                sb.AppendLine("\n<strong>Poloniex commands</strong>");
                sb.AppendLine();
                sb.AppendLine($"{TelegramCommands.PoloniexBalanceInfo} - poloniex account summary");
                sb.AppendLine($"{TelegramCommands.PoloniexTradeReset} - reset trades database from poloniex");
            }

            _log.LogInformation("Sending help message");

            await _bus.SendAsync(new SendMessageCommand(sb));
        }
    }

    public class SendHelpMessageCommand : ICommand
    {
    }
}