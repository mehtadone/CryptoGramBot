using System;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Commands;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Helpers;
using CryptoGramBot.Services;
using CryptoGramBot.Services.Exchanges;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers.Poloniex
{
    public class PoloniexBalanceCheckHandler : IEventHandler<BalanceCheckEvent>
    {
        private readonly IMicroBus _bus;
        private readonly PoloniexConfig _config;
        private readonly PoloniexService _poloniexService;

        public PoloniexBalanceCheckHandler(PoloniexService poloniexService, PoloniexConfig config, IMicroBus bus)
        {
            _poloniexService = poloniexService;
            _config = config;
            _bus = bus;
        }

        public async Task Handle(BalanceCheckEvent @event)
        {
            if (@event.Exchange == null || @event.Exchange == Constants.Poloniex)
            {
                var balanceInformation = await _poloniexService.GetBalance();

                var dailyBalance = _config.DailyNotifications.Split(':');
                int.TryParse(dailyBalance[0], out int hour);
                int.TryParse(dailyBalance[1], out int min);

                if (_config.SendHourlyUpdates || @event.UserRequested || (dailyBalance.Length == 2 && DateTime.Now.Hour == hour && DateTime.Now.Minute == min))
                {
                    await _bus.SendAsync(new SendBalanceInfoCommand(balanceInformation));
                }
            }
        }
    }
}