using System.Threading.Tasks;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.EventBus
{
    public class HelpMessageHandler : ICommandHandler<SendHelpMessageCommand>
    {
        private readonly IMicroBus _bus;
        private readonly ILogger<HelpMessageHandler> _log;

        public HelpMessageHandler(ILogger<HelpMessageHandler> log, IMicroBus bus)
        {
            _log = log;
            _bus = bus;
        }

        public async Task Handle(SendHelpMessageCommand command)
        {
            var usage = "Usage:\n" +
                        "/acc_n - 24 hour pnl for account number n.\n" +
                        "/list_coinigy_accounts - all coinigy account names\n" +
                        "/total - 24 hour PnL for the total balance\n" +
                        "/excel - an excel export of all trades\n" +
                        "/profit BTC-XXX - profit information for pair\n" +
                        "/upload_bittrex_orders - upload bittrex order export";
            _log.LogInformation("Sending help message");

            await _bus.SendAsync(new SendMessageCommand(usage));
        }
    }

    public class SendHelpMessageCommand : ICommand
    {
    }
}