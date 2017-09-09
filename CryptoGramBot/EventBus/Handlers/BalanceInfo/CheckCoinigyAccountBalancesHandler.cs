using System.Threading.Tasks;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers
{
    public class CheckCoinigyAccountBalancesHandler : IEventHandler<BalanceCheckEvent>
    {
        private readonly CoinigyBalanceService _coinigyBalanceService;

        public CheckCoinigyAccountBalancesHandler(CoinigyBalanceService coinigyBalanceService)
        {
            _coinigyBalanceService = coinigyBalanceService;
        }

        public async Task Handle(BalanceCheckEvent command)
        {
            await _coinigyBalanceService.GetAllBalances();
        }
    }
}