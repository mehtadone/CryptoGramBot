using System;
using System.Linq;
using System.Threading.Tasks;
using CryptoGramBot.EventBus;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.Services
{
    public class CoinigyAccountInfoHandler : ICommandHandler<SendCoinigyAccountInfoCommand>
    {
        private readonly BalanceService _balanceService;
        private readonly IMicroBus _bus;
        private readonly ILogger<CoinigyAccountInfoHandler> _log;

        public CoinigyAccountInfoHandler(BalanceService balanceService, ILogger<CoinigyAccountInfoHandler> log, IMicroBus bus)
        {
            _balanceService = balanceService;
            _log = log;
            _bus = bus;
        }

        public async Task Handle(SendCoinigyAccountInfoCommand command)
        {
            var accountList = await _balanceService.GetAccounts();
            var message = accountList.Aggregate($"{DateTime.Now:g}\n" + "Connected accounts on Coinigy are: \n", (current, acc) => current + "/acc_" + acc.Key + " - " + acc.Value.Name + "\n");
            _log.LogInformation("Sending the account list");
            await _bus.SendAsync(new SendMessageCommand(message));
        }
    }

    public class SendCoinigyAccountInfoCommand : ICommand
    {
    }
}