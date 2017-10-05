using System;
using System.Linq;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using CryptoGramBot.Services;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.EventBus.Handlers.Poloniex
{
    public class PoloniexNewOrderCheckHandler : IEventHandler<NewTradesCheckEvent>
    {
        private readonly IMicroBus _bus;
        private readonly IConfig _config;
        private readonly ILogger<PoloniexService> _log;
        private readonly PoloniexService _poloService;

        public PoloniexNewOrderCheckHandler(PoloniexService poloService, ILogger<PoloniexService> log, IMicroBus bus, PoloniexConfig config)
        {
            _poloService = poloService;
            _log = log;
            _bus = bus;
            _config = config;
        }

        public async Task Handle(NewTradesCheckEvent @event)
        {
            try
            {
                var lastChecked = await _bus.QueryAsync(new LastCheckedQuery(Constants.Poloniex));
                var orderHistory = await _poloService.GetOrderHistory(lastChecked.LastChecked - TimeSpan.FromDays(2));
                var newTradesResponse = await _bus.QueryAsync(new FindNewTradeQuery(orderHistory));
                await _bus.SendAsync(new AddLastCheckedCommand(Constants.Poloniex));

                await SendAndCheckNotifications(newTradesResponse);
                await SendOpenOrdersNotifications(lastChecked.LastChecked);
            }
            catch (Exception ex)
            {
                _log.LogError("Error in getting new orders from poloniex\n" + ex.Message);
                throw;
            }
        }

        private async Task SendAndCheckNotifications(FindNewTradesResponse newTradesResponse)
        {
            if (newTradesResponse.NewTrades.Count() > 10)
            {
                await _bus.SendAsync(
                    new SendMessageCommand(
                        "There are more than 10 Poloniex trades to send. Not going to send them to avoid spamming you"));
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
                var newOrders = await _poloService.GetNewOpenOrders(lastChecked - TimeSpan.FromDays(2));

                if (newOrders.Count > 5)
                {
                    await _bus.SendAsync(new SendMessageCommand($"{newOrders.Count} open poloniex orders available to send. Will not send them to avoid spam."));
                    return;
                }

                foreach (var openOrder in newOrders)
                {
                    var message = $"{openOrder.Opened:g}\n" +
                                  $"New {openOrder.Exchange} OPEN order\n" +
                                  $"<strong>{openOrder.Side} {openOrder.Base}-{openOrder.Terms}</strong>\n" +
                                  $"Price: {openOrder.Price}\n" +
                                  $"Quanitity: {openOrder.Quantity}";
                    await _bus.SendAsync(new SendMessageCommand(message));
                }
            }
        }
    }
}