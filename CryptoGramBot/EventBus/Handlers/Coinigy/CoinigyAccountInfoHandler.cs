using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.Services;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.EventBus.Handlers
{
    public class CoinigyAccountInfoHandler : ICommandHandler<SendCoinigyAccountInfoCommand>
    {
        private readonly IMicroBus _bus;
        private readonly CoinigyBalanceService _coinigyBalanceService;
        private readonly ILogger<CoinigyAccountInfoHandler> _log;

        public CoinigyAccountInfoHandler(CoinigyBalanceService coinigyBalanceService, ILogger<CoinigyAccountInfoHandler> log, IMicroBus bus)
        {
            _coinigyBalanceService = coinigyBalanceService;
            _log = log;
            _bus = bus;
        }

        public async Task Handle(SendCoinigyAccountInfoCommand command)
        {
            var accountList = await _coinigyBalanceService.GetAccounts();
            var sb = new StringBuilder();
            sb.AppendLine($"{DateTime.Now:g}");
            sb.AppendLine("Connected accounts on Coinigy are:");
            foreach (var pair in accountList)
            {
                sb.Append("/acc_");
                sb.Append(pair.Key.ToString());
                sb.Append(" - ");
                sb.Append(pair.Value.Name);
                sb.AppendLine();
            }
            _log.LogInformation("Sending the account list");
            await _bus.SendAsync(new SendMessageCommand(sb));
        }
    }

    public class SendCoinigyAccountInfoCommand : ICommand
    {
    }
}