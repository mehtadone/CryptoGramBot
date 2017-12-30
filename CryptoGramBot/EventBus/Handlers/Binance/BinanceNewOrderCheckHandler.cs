using System;
using System.Linq;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using CryptoGramBot.Services.Exchanges;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.EventBus.Handlers.Binance
{
    public class BinanceNewOrderCheckHandler : IEventHandler<NewTradesCheckEvent>
    {
        private readonly IMicroBus _bus;
        private readonly IConfig _config;
        private readonly BinanceService _exchangeService;
        private readonly ILogger<BinanceService> _log;

        public BinanceNewOrderCheckHandler(BinanceService exchangeService, ILogger<BinanceService> log, IMicroBus bus, BinanceConfig config)
        {
            _exchangeService = exchangeService;
            _log = log;
            _bus = bus;
            _config = config;
        }

        public async Task Handle(NewTradesCheckEvent @event)
        {
            try
            {
                var lastChecked = await _bus.QueryAsync(new LastCheckedQuery(Constants.Binance));
                var orderHistory = await _exchangeService.GetOrderHistory(lastChecked.LastChecked - TimeSpan.FromDays(2));
                var newTradesResponse = await _bus.QueryAsync(new FindNewTradeQuery(orderHistory));
                await _bus.SendAsync(new AddLastCheckedCommand(Constants.Binance));

                await SendAndCheckNotifications(newTradesResponse);
                await SendOpenOrdersNotifications(lastChecked.LastChecked);
            }
            catch (Exception ex)
            {
                _log.LogError("Error in getting new orders from Binance\n" + ex.Message);
                throw;
            }
        }

        private async Task SendAndCheckNotifications(FindNewTradesResponse newTradesResponse)
        {
            if (!_config.BuyNotifications.HasValue && !_config.SellNotifications.HasValue) return;

            var count = newTradesResponse.NewTrades.Count();
            if (count > 29)
            {
                var stringBuffer = new StringBuffer();
                stringBuffer.Append(StringContants.BinanceMoreThan30Trades);
                await _bus.SendAsync(
                    new SendMessageCommand(
                        stringBuffer));
                return;
            }

            foreach (var newTrade in newTradesResponse.NewTrades)
            {
                if (newTrade.Side == TradeSide.Sell && _config.SellNotifications.HasValue && _config.SellNotifications.Value)
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
                var newOrders = await _exchangeService.GetNewOpenOrders(lastChecked - TimeSpan.FromDays(2));

                if (newOrders.Count > 30)
                {
                    var stringBuilder = new StringBuffer();
                    stringBuilder.Append(StringContants.BinanceMoreThan30OpenOrders);

                    await _bus.SendAsync(new SendMessageCommand(stringBuilder));
                    return;
                }

                foreach (var openOrder in newOrders)
                {
                    var sb = new StringBuffer();
                    sb.Append($"{openOrder.Opened:g}\n");
                    sb.Append($"New {openOrder.Exchange} OPEN order\n");
                    sb.Append($"{StringContants.StrongOpen}{openOrder.Side} {openOrder.Base}-{openOrder.Terms}{StringContants.StrongClose}\n");
                    sb.Append($"Price: {openOrder.Price}\n");
                    sb.Append($"Quanitity: {openOrder.Quantity}");
                    await _bus.SendAsync(new SendMessageCommand(sb));
                }
            }
        }
    }
}