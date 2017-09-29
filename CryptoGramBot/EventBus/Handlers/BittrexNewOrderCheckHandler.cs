using System;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers
{
    public class BittrexNewOrderCheckHandler : IEventHandler<NewTradesCheckEvent>
    {
        private readonly BittrexService _bittrexService;
        private readonly IMicroBus _bus;
        private readonly IConfig _config;

        public BittrexNewOrderCheckHandler(BittrexService bittrexService, IMicroBus bus, BittrexConfig config)
        {
            _bittrexService = bittrexService;
            _bus = bus;
            _config = config;
        }

        public async Task Handle(NewTradesCheckEvent @event)
        {
            var lastChecked = await _bus.QueryAsync(new LastCheckedQuery(Constants.Bittrex));
            var orderHistory = await _bittrexService.GetOrderHistory(lastChecked.LastChecked);
            var newTradesResponse = await _bus.QueryAsync(new FindNewTradeQuery(orderHistory));
            await _bus.SendAsync(new AddLastCheckedCommand(Constants.Bittrex));

            var i = 0;

            if (!_config.BuyNotifications && !_config.SellNotifications) return;

            foreach (var newTrade in newTradesResponse.NewTrades)
            {
                if (@event.IsStartup && i > 4) break;
                if (newTrade.Side == TradeSide.Sell && _config.SellNotifications)
                {
                    await _bus.SendAsync(new TradeNotificationCommand(newTrade));
                }

                if (newTrade.Side == TradeSide.Buy && _config.BuyNotifications)
                {
                    await _bus.SendAsync(new TradeNotificationCommand(newTrade));
                }

                i++;
            }
        }
    }
}