using System;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
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
        private readonly GeneralConfig _config;

        public TradeNotificationHandler(IMicroBus bus, GeneralConfig config)
        {
            _bus = bus;
            _config = config;
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
                var tradesProfitResponse = await _bus.QueryAsync(new TradeProfitQuery(newTrade.Cost, newTrade.QuantityOfTrade, newTrade.Base, newTrade.Terms, newTrade.Exchange));
                profitPercentage = tradesProfitResponse.ProfitPercentage;
                btcProfit = tradesProfitResponse.BtcProfit;
                dollarProfit = tradesProfitResponse.DollarProfit;
                lastBought = tradesProfitResponse.LastBoughtTime + TimeSpan.FromHours(_config.TimeOffset);
            }

            var sb = new StringBuilder();

            sb.AppendLine($"{newTrade.TimeStamp + TimeSpan.FromHours(_config.TimeOffset):g}");
            sb.AppendLine($"New {newTrade.Exchange} order");
            sb.AppendLine($"<strong>{newTrade.Side} {newTrade.Base}-{newTrade.Terms}</strong>");
            sb.AppendLine($"Quantity: {newTrade.QuantityOfTrade}");
            sb.AppendLine($"Rate: {newTrade.Limit:##0.##############} {newTrade.Base}");
            sb.AppendLine($"Total: {newTrade.Cost:##0.###########} {newTrade.Base}");

            if (profitPercentage.HasValue && btcProfit.HasValue && dollarProfit.HasValue)
            {
                sb.AppendLine($"Profit: {btcProfit.Value:##0.########} {newTrade.Base} (${dollarProfit.Value:###0.##})");
                sb.AppendLine($"Bought on: {lastBought:g}");
                sb.AppendLine($"<strong>Percentage: {profitPercentage.Value}%</strong>");
            }

            await _bus.SendAsync(new SendMessageCommand(sb));
        }
    }
}