using System.Threading.Tasks;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers
{
    public class CheckCoinigyAccountBalancesCommand : ICommand
    {
    }

    public class CheckCoinigyAccountBalancesHandler : ICommandHandler<CheckCoinigyAccountBalancesCommand>
    {
        private readonly CoinigyBalanceService _coinigyBalanceService;

        public CheckCoinigyAccountBalancesHandler(CoinigyBalanceService coinigyBalanceService)
        {
            _coinigyBalanceService = coinigyBalanceService;
        }

        public async Task Handle(CheckCoinigyAccountBalancesCommand command)
        {
            await _coinigyBalanceService.GetAllBalances();
        }
    }
}