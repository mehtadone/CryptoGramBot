using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers.BalanceInfo
{
    public class BittrexBalanceCheckHandler : IEventHandler<BalanceCheckEvent>
    {
        private readonly BittrexService _bittrexService;
        private readonly BittrexConfig _config;

        public BittrexBalanceCheckHandler(BittrexService bittrexService, BittrexConfig config)
        {
            _bittrexService = bittrexService;
            _config = config;
        }

        public async Task Handle(BalanceCheckEvent @event)
        {
            await _bittrexService.GetBalance(_config.Name);
        }
    }
}