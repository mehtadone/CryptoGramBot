using System;
using System.Linq;
using System.Text;
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
            if (!_config.BuyNotifications && !_config.SellNotifications) return;

            if (newTradesResponse.NewTrades.Count() > 29)
            {
                await _bus.SendAsync(
                    new SendMessageCommand(new StringBuilder("There are more than 30 Bittrex trades to send. Not going to send them to avoid spamming you")));
                return;
            }

            foreach (var newTrade in newTradesResponse.NewTrades)
            {
                if (newTrade.Side == TradeSide.Sell && _config.SellNotifications)
                {
                    await _bus.SendAsync(new TradeNotificationCommand(newTrade));
                }

                if (newTrade.Side == TradeSide.Buy && _config.BuyNotifications)
                {
                    await _bus.SendAsync(new TradeNotificationCommand(newTrade));
                }
            }
        }

        private async Task SendOpenOrdersNotifications(DateTime lastChecked)
        {
            if (_config.OpenOrderNotification)
            {
                var newOrders = await _bittrexService.GetNewOpenOrders(lastChecked);

                if (newOrders.Count > 5)
                {
                    await _bus.SendAsync(new SendMessageCommand(new StringBuilder($"{newOrders.Count} open Bittrex orders available to send. Will not send them to avoid spam.")));
                    return;
                }
                foreach (var openOrder in newOrders)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"{openOrder.Opened:g}");
                    sb.AppendLine($"New {openOrder.Exchange} OPEN order");
                    sb.AppendLine($"<strong>{openOrder.Side} {openOrder.Base}-{openOrder.Terms}</strong>");
                    sb.AppendLine($"Price: {openOrder.Price}");
                    sb.AppendLine($"Quanitity: {openOrder.Quantity}");
                    await _bus.SendAsync(new SendMessageCommand(sb));
                }
            }
        }
    }
}