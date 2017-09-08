using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.EventBus;
using CryptoGramBot.EventBus.Handlers;
using CryptoGramBot.Models;
using Enexure.MicroBus;

namespace CryptoGramBot.Services
{
    public class TradeNotificationCommand : ICommand
    {
        public TradeNotificationCommand(Trade newTrade)
        {
            NewTrade = newTrade;
        }

        public Trade NewTrade { get; }
    }

    public class TradeNotificationHandler : ICommandHandler<TradeNotificationCommand>
    {
        private readonly IMicroBus _bus;

        public TradeNotificationHandler(IMicroBus bus)
        {
            _bus = bus;
        }

        public async Task Handle(TradeNotificationCommand command)
        {
            var newTrade = command.NewTrade;

            decimal? profit = null;
            if (newTrade.Side == TradeSide.Sell)
            {
                var tradesProfitResponse = await _bus.QueryAsync(new TradeProfitQuery(newTrade.Cost, newTrade.Quantity, newTrade.Base, newTrade.Terms));
                profit = tradesProfitResponse.ProfitPercentage;
            }

            var message = $"{newTrade.TimeStamp:R}\n" +
                          $"New {newTrade.Exchange} order\n" +
                          $"<strong>{newTrade.Side} {newTrade.Base}-{newTrade.Terms}</strong>\n" +
                          $"Total: {newTrade.Cost:##0.###########} BTC\n" +
                          $"Rate: {newTrade.Limit:##0.##############} BTC";

            if (profit.HasValue)
            {
                message = message + $"\nProfit: {profit.Value} %";
            }

            await _bus.SendAsync(new SendMessageCommand(message));
        }
    }
}