using System;
using System.Threading.Tasks;
using CryptoGramBot.Models;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers
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

            decimal? profitPercentage = null;
            decimal? btcProfit = null;
            decimal? dollarProfit = null;
            DateTime? lastBought = DateTime.MinValue;
            ;

            if (newTrade.Side == TradeSide.Sell)
            {
                var tradesProfitResponse = await _bus.QueryAsync(new TradeProfitQuery(newTrade.Cost, newTrade.QuantityOfTrade, newTrade.Base, newTrade.Terms));
                profitPercentage = tradesProfitResponse.ProfitPercentage;
                btcProfit = tradesProfitResponse.BtcProfit;
                dollarProfit = tradesProfitResponse.DollarProfit;
                lastBought = tradesProfitResponse.LastBoughtTime;
            }

            var message = $"{newTrade.TimeStamp:g}\n" +
                          $"New {newTrade.Exchange} order\n" +
                          $"<strong>{newTrade.Side} {newTrade.Base}-{newTrade.Terms}</strong>\n" +
                          $"Total: {newTrade.Cost:##0.###########} {newTrade.Base}\n" +
                          $"Rate: {newTrade.Limit:##0.##############} {newTrade.Base}";

            if (profitPercentage.HasValue && btcProfit.HasValue && dollarProfit.HasValue)
            {
                message = message + $"\nProfit: {btcProfit.Value:##0.###########} {newTrade.Base} (${dollarProfit.Value:###0.##})\n"
                    + $"Bought on: {lastBought:g}\n"
                    + $"Percentage: {profitPercentage.Value}%";
            }

            await _bus.SendAsync(new SendMessageCommand(message));
        }
    }
}