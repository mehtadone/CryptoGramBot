using System;
using System.Threading.Tasks;
using CryptoGramBot.Helpers;
using CryptoGramBot.Services.Exchanges;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.EventBus.Handlers.Coinigy
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
            var sb = new StringBuffer();
            sb.Append(string.Format("{0}\n", DateTime.Now.ToString("g")));
            sb.Append(StringContants.CoinigyConnectedAccounts + "\n");
            foreach (var pair in accountList)
            {
                sb.Append("/acc_");
                sb.Append(pair.Key.ToString());
                sb.Append(" - ");
                sb.Append(pair.Value.Name);
                sb.Append("\n");
            }
            _log.LogInformation("Sending the account list");
            await _bus.SendAsync(new SendMessageCommand(sb));
        }
    }

    public class SendCoinigyAccountInfoCommand : ICommand
    {
    }
}