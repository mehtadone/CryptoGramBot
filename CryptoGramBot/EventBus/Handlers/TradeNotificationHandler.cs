using System;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
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
            decimal? reportingProfit = null;
            string reportingCurrency = null;
            DateTime? lastBought = DateTime.MinValue;
            ;

            if (newTrade.Side == TradeSide.Sell)
            {
                var tradesProfitResponse = await _bus.QueryAsync(new TradeProfitQuery(newTrade.Cost, newTrade.QuantityOfTrade, newTrade.Base, newTrade.Terms, newTrade.Exchange));
                profitPercentage = tradesProfitResponse.ProfitPercentage;
                btcProfit = tradesProfitResponse.BtcProfit;
                reportingProfit = tradesProfitResponse.ReportingProfit;
                reportingCurrency = tradesProfitResponse.ReportingCurrency;
                lastBought = tradesProfitResponse.LastBoughtTime;
            }

            var sb = new StringBuffer();

            sb.Append(string.Format("{0}\n", (newTrade.Timestamp + TimeSpan.FromHours(_config.TimeOffset)).ToString("g")));
            sb.Append(string.Format("New {0} order\n", newTrade.Exchange));
            sb.Append(string.Format("{3}{0} {1}-{2}{4}\n", newTrade.Side.ToString(), newTrade.Base, newTrade.Terms, StringContants.StrongOpen, StringContants.StrongClose));
            sb.Append(string.Format("Quantity: {0}\n", newTrade.QuantityOfTrade.ToString("##0.###########")));
            sb.Append(string.Format("Rate: {0} {1}\n", newTrade.Limit.ToString("##0.##############"), newTrade.Base));
            sb.Append(string.Format("Total: {0} {1}\n", newTrade.Cost.ToString("##0.###########"), newTrade.Base));

            if (profitPercentage.HasValue && btcProfit.HasValue && reportingProfit.HasValue && reportingCurrency != null)
            {
                sb.Append(string.Format("Profit: {0} {1} ({2})\n", btcProfit.Value.ToString("##0.########"), newTrade.Base, Helpers.Helpers.FormatCurrencyAmount(reportingProfit.Value, reportingCurrency)));
                sb.Append(string.Format("Bought on: {0}\n", (lastBought.Value + TimeSpan.FromHours(_config.TimeOffset)).ToString("g")));
                sb.Append(string.Format("{1}Percentage: {0}%{2}\n", profitPercentage.Value, StringContants.StrongOpen, StringContants.StrongClose));
            }

            await _bus.SendAsync(new SendMessageCommand(sb));
        }
    }
}