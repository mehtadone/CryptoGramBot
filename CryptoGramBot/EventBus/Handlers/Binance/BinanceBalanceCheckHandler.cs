using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Commands;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Helpers;
using CryptoGramBot.Services.Exchanges;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers.Binance
{
    public class BinanceBalanceCheckHandler : IEventHandler<BalanceCheckEvent>
    {
        private readonly IMicroBus _bus;
        private readonly BinanceConfig _config;
        private readonly BinanceService _exchangeService;

        public BinanceBalanceCheckHandler(BinanceService exchangeService, BinanceConfig config, IMicroBus bus)
        {
            _exchangeService = exchangeService;
            _config = config;
            _bus = bus;
        }

        public async Task Handle(BalanceCheckEvent @event)
        {
            if (@event.Exchange == null || @event.Exchange == Constants.Binance)
            {
                var balanceInformation = await _exchangeService.GetBalance();

                if (@event.UserRequested)
                {
                    await _bus.SendAsync(new SendBalanceInfoCommand(balanceInformation));
                }
            }
        }
    }
}