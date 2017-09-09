using System.Threading.Tasks;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Helpers;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers.BalanceInfo
{
    public class PoloniexBalanceCheckHandler : IEventHandler<BalanceCheckEvent>
    {
        private readonly PoloniexService _poloniexService;

        public PoloniexBalanceCheckHandler(PoloniexService poloniexService)
        {
            _poloniexService = poloniexService;
        }

        public async Task Handle(BalanceCheckEvent @event)
        {
            await _poloniexService.GetBalance();
        }
    }
}