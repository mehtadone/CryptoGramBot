using System;
using System.Threading.Tasks;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Helpers;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers
{
    public class BittrexNewOrderCheckHandler : IEventHandler<NewTradesCheckEvent>
    {
        private readonly BittrexService _bittrexService;
        private readonly IMicroBus _bus;

        public BittrexNewOrderCheckHandler(BittrexService bittrexService, IMicroBus bus)
        {
            _bittrexService = bittrexService;
            _bus = bus;
        }

        public async Task Handle(NewTradesCheckEvent @event)
        {
            var lastChecked = await _bus.QueryAsync(new LastCheckedQuery(Constants.Bittrex));
            var orderHistory = await _bittrexService.GetOrderHistory(lastChecked.LastChecked);
            var newTradesResponse = await _bus.QueryAsync(new FindNewTradeQuery(orderHistory));
            await _bus.SendAsync(new AddLastCheckedCommand(Constants.Bittrex));

            if (@event.BittrexTradeNotifcations)
            {
                var i = 0;
                foreach (var newTrade in newTradesResponse.NewTrades)
                {
                    if (@event.IsStartup && i > 4) break;
                    await _bus.SendAsync(new TradeNotificationCommand(newTrade));
                    i++;
                }
            }
        }
    }
}