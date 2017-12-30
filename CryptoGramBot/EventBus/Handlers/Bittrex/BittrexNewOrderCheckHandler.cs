using System;
using System.Linq;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using CryptoGramBot.Services.Exchanges;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers.Bittrex
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

            await SendAndCheckNotifications(newTradesResponse);
            await SendOpenOrdersNotifications(lastChecked.LastChecked);
        }

        private async Task SendAndCheckNotifications(FindNewTradesResponse newTradesResponse)
        {
            if (!_config.BuyNotifications.HasValue && !_config.SellNotifications.HasValue) return;

            if (newTradesResponse.NewTrades.Count() > 29)
            {
                var stringBuilder = new StringBuffer();
                stringBuilder.Append(StringContants.BittrexMoreThan30Trades);
                await _bus.SendAsync(
                    new SendMessageCommand(stringBuilder));
                return;
            }

            foreach (var newTrade in newTradesResponse.NewTrades)
            {
                if (newTrade.Side == TradeSide.Sell && _config.SellNotifications.HasValue)
                {
                    await _bus.SendAsync(new TradeNotificationCommand(newTrade));
                }

                if (newTrade.Side == TradeSide.Buy && _config.BuyNotifications.HasValue && _config.BuyNotifications.Value)
                {
                    await _bus.SendAsync(new TradeNotificationCommand(newTrade));
                }
            }
        }

        private async Task SendOpenOrdersNotifications(DateTime lastChecked)
        {
            if (_config.OpenOrderNotification.HasValue && _config.OpenOrderNotification.Value)
            {
                var newOrders = await _bittrexService.GetNewOpenOrders(lastChecked);

                if (newOrders.Count > 30)
                {
                    var stringBuilder = new StringBuffer();
                    stringBuilder.Append(StringContants.BittrexMoreThan30OpenOrders);
                    await _bus.SendAsync(new SendMessageCommand(stringBuilder));
                    return;
                }
                foreach (var openOrder in newOrders)
                {
                    var sb = new StringBuffer();
                    sb.Append(string.Format("{0}", openOrder.Opened.ToString("g")));
                    sb.Append(string.Format("New {0} OPEN order", openOrder.Exchange));
                    sb.Append(string.Format("{3}{0} {1}-{2}{4}", openOrder.Side, openOrder.Base, openOrder.Terms, StringContants.StrongOpen, StringContants.StrongClose));
                    sb.Append(string.Format("Price: {0}", openOrder.Price));
                    sb.Append(string.Format("Quanitity: {0}", openOrder.Quantity));
                    await _bus.SendAsync(new SendMessageCommand(sb));
                }
            }
        }
    }
}