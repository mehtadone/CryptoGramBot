using System.Threading.Tasks;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Helpers;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers.BalanceInfo
{
    public class CheckCoinigyTotalBalanceHandler : IEventHandler<BalanceCheckEvent>
    {
        private readonly CoinigyBalanceService _coinigyBalanceService;

        public CheckCoinigyTotalBalanceHandler(CoinigyBalanceService coinigyBalanceService)
        {
            _coinigyBalanceService = coinigyBalanceService;
        }

        public async Task Handle(BalanceCheckEvent command)
        {
            await _coinigyBalanceService.GetBalance(Constants.TotalCoinigyBalance);
        }
    }
}