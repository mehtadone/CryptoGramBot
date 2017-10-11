using System;
using System.Linq;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Commands;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Helpers;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers.BalanceInfo
{
    public class CoinigyBalanceHandler : IEventHandler<BalanceCheckEvent>
    {
        private readonly IMicroBus _bus;
        private readonly CoinigyBalanceService _coinigyBalanceService;
        private readonly CoinigyConfig _config;

        public CoinigyBalanceHandler(
            CoinigyBalanceService coinigyBalanceService,
            CoinigyConfig config,
            IMicroBus bus
            )
        {
            _coinigyBalanceService = coinigyBalanceService;
            _config = config;
            _bus = bus;
        }

        public async Task Handle(BalanceCheckEvent @event)
        {
            if (@event.Exchange == null && !@event.UserRequested)
            {
                await _coinigyBalanceService.GetAllBalances();
                await _coinigyBalanceService.GetBalance();
            }

            var dailyBalance = _config.DailyNotifications.Split(':');
            int.TryParse(dailyBalance[0], out int hour);
            int.TryParse(dailyBalance[1], out int min);

            if ((@event.UserRequested || _config.SendHourlyUpdates || (dailyBalance.Length == 2 && DateTime.Now.Hour == hour && DateTime.Now.Minute == min)) && @event.Exchange == Constants.CoinigyAccountBalance)
            {
                if (@event.CoinigyAccountId.HasValue)
                {
                    var accountInfo = await _coinigyBalanceService.GetAccountBalance(@event.CoinigyAccountId.Value);
                    await _bus.SendAsync(new SendBalanceInfoCommand(accountInfo));
                }
                else
                {
                    await _bus.SendAsync(new SendHelpMessageCommand());
                }
            }

            if (@event.Exchange == Constants.TotalCoinigyBalance && (@event.UserRequested || _config.SendHourlyUpdates || (dailyBalance.Length == 2 && DateTime.Now.Hour == hour && DateTime.Now.Minute == min)))
            {
                await _coinigyBalanceService.GetAllBalances();
                var balanceInformation = await _coinigyBalanceService.GetBalance();
                await _bus.SendAsync(new SendBalanceInfoCommand(balanceInformation));
            }
        }
    }
}