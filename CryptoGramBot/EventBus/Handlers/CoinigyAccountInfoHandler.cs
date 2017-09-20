using System;
using System.Linq;
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
            var message = accountList.Aggregate($"{DateTime.Now:g}\n" + "Connected accounts on Coinigy are: \n", (current, acc) => current + "/acc_" + acc.Key + " - " + acc.Value.Name + "\n");
            _log.LogInformation("Sending the account list");
            await _bus.SendAsync(new SendMessageCommand(message));
        }
    }

    public class SendCoinigyAccountInfoCommand : ICommand
    {
    }
}