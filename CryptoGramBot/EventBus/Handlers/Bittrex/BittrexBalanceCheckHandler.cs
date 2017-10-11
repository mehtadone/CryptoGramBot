using System;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Commands;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Helpers;
using CryptoGramBot.Services;
using CryptoGramBot.Services.Exchanges;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers.Bittrex
{
    public class BittrexBalanceCheckHandler : IEventHandler<BalanceCheckEvent>
    {
        private readonly BittrexService _bittrexService;
        private readonly IMicroBus _bus;
        private readonly BittrexConfig _config;

        public BittrexBalanceCheckHandler(BittrexService bittrexService,
            BittrexConfig config, IMicroBus bus)
        {
            _bittrexService = bittrexService;
            _config = config;
            _bus = bus;
        }

        public async Task Handle(BalanceCheckEvent @event)
        {
            if (@event.Exchange == null || @event.Exchange == Constants.Bittrex)
            {
                var balanceInformation = await _bittrexService.GetBalance();

                if (_config.SendHourlyUpdates || @event.UserRequested)
                {
                    await _bus.SendAsync(new SendBalanceInfoCommand(balanceInformation));
                }
            }
        }
    }
}