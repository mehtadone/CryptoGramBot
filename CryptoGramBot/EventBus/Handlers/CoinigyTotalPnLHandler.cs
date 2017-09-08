using System.Threading.Tasks;
using CryptoGramBot.Helpers;
using CryptoGramBot.Services;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.EventBus.Handlers
{
    public class CoinigyTotalPnLCommand : ICommand
    {
    }

    public class CoinigyTotalPnLHandler : ICommandHandler<CoinigyTotalPnLCommand>
    {
        private readonly IMicroBus _bus;
        private readonly CoinigyBalanceService _coinigyBalanceService;
        private readonly ILogger<CoinigyTotalPnLHandler> _log;

        public CoinigyTotalPnLHandler(CoinigyBalanceService coinigyBalanceService, ILogger<CoinigyTotalPnLHandler> log, IMicroBus bus)
        {
            _coinigyBalanceService = coinigyBalanceService;
            _log = log;
            _bus = bus;
        }

        public async Task Handle(CoinigyTotalPnLCommand command)
        {
            var balance = await _coinigyBalanceService.Get24HourTotalBalance();
            _log.LogInformation("Sending total pnl");
            await _bus.SendAsync(new CoinigyBalanceUpdateCommand(balance.CurrentBalance, balance.PreviousBalance, Constants.TotalCoinigyBalance));
        }
    }
}