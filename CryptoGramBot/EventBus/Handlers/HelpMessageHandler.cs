using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.EventBus.Handlers
{
    public class HelpMessageHandler : ICommandHandler<SendHelpMessageCommand>
    {
        private readonly BinanceConfig _binanceConfig;
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
            BinanceConfig binanceConfig,
            IMicroBus bus)
        {
            _log = log;
            _bittrexConfig = bittrexConfig;
            _poloniexConfig = poloniexConfig;
            _coinigyConfig = coinigyConfig;
            _binanceConfig = binanceConfig;
            _bus = bus;
        }

        public async Task Handle(SendHelpMessageCommand command)
        {
            var sb = new StringBuffer();
            sb.Append(StringContants.Help);
            sb.Append(StringContants.CommonCommands);
            sb.Append(string.Format("{0} - an excel export of all trades\n", TelegramCommands.CommonExcel));
            sb.Append(string.Format("{0} - profit information for pair\n", TelegramCommands.CommonPairProfit));

            if (_coinigyConfig.Enabled)
            {
                sb.Append(StringContants.CoinigyCommands);
                sb.Append(string.Format("{0} - Coinigy accounts and their balance\n", TelegramCommands.CoinigyAccountList));
                sb.Append(string.Format("{0} - total balance from all acounts\n", TelegramCommands.CoinigyTotalBalance));
            }

            if (_bittrexConfig.Enabled)
            {
                sb.Append(StringContants.BittrexCommands);
                sb.Append(string.Format("{0} - upload Bittrex order export\n", TelegramCommands.BittrexTradeExportUpload));
                sb.Append(string.Format("{0} - Bittrex account summary\n", TelegramCommands.BittrexBalanceInfo));
            }
            if (_poloniexConfig.Enabled)
            {
                sb.Append(StringContants.PoloCommands);
                sb.Append(string.Format("{0} - Poloniex account summary\n", TelegramCommands.PoloniexBalanceInfo));
                sb.Append(string.Format("{0} - reset trades database from Poloniex\n", TelegramCommands.PoloniexTradeReset));
            }
            if (_binanceConfig.Enabled)
            {
                sb.Append(StringContants.BinanceCommands);
                sb.Append(string.Format("{0} - Binance account summary\n", TelegramCommands.BinanceBalanceInfo));
            }

            _log.LogInformation("Sending help message");

            await _bus.SendAsync(new SendMessageCommand(sb));
        }
    }

    public class SendHelpMessageCommand : ICommand
    {
    }
}