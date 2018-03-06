using Enexure.MicroBus;
using System;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using CryptoGramBot.Services.Data;
using CryptoGramBot.Services.Pricing;

namespace CryptoGramBot.EventBus.Handlers
{
    public class TradeProfitHandler : IQueryHandler<TradeProfitQuery, TradesProfitResponse>
    {
        private readonly DatabaseService _databaseService;
        private readonly PriceService _priceService;
        private readonly GeneralConfig _config;

        public TradeProfitHandler(DatabaseService databaseService, PriceService priceService, GeneralConfig config)
        {
            _databaseService = databaseService;
            _priceService = priceService;
            _config = config;
        }

        public async Task<TradesProfitResponse> Handle(TradeProfitQuery query)
        {
            var averagePrice = await _databaseService.GetBuyAveragePrice(query.BaseCcy, query.Terms, query.Exchange, query.Quantity);

            if (averagePrice == 0m)
            {
                return new TradesProfitResponse(null, null, null, null, null);
            }

            var totalCost = averagePrice * query.Quantity;
            var profit = ProfitCalculator.GetProfitForSell(query.SellReturns, query.Quantity, averagePrice, totalCost);

            var lastBought = await _databaseService.GetLastBoughtAsync(query.BaseCcy, query.Terms, query.Exchange);

            decimal? btcProfit = query.SellReturns - totalCost;
            string reportingCurrency = _config.ReportingCurrency;
            decimal? reportingProfit = await _priceService.GetReportingAmount(query.BaseCcy, btcProfit.Value, reportingCurrency, query.Exchange);
            return new TradesProfitResponse(profit, btcProfit, reportingProfit, reportingCurrency, lastBought);
        }
    }

    public class TradeProfitQuery : IQuery<TradeProfitQuery, TradesProfitResponse>
    {
        public TradeProfitQuery(decimal sellReturns, decimal quantity, string baseCcy, string terms, string exchange)
        {
            SellReturns = sellReturns;
            Quantity = quantity;
            BaseCcy = baseCcy;
            Terms = terms;
            Exchange = exchange;
        }

        public string BaseCcy { get; }
        public string Exchange { get; }
        public decimal Quantity { get; }
        public decimal SellReturns { get; }
        public string Terms { get; }
    }

    public class TradesProfitResponse
    {
        public TradesProfitResponse(decimal? profitPercentage, decimal? btcProfit, decimal? reportingProfit, string reportingCurrency, DateTime? lastBoughtTime)
        {
            ProfitPercentage = profitPercentage;
            BtcProfit = btcProfit;
            ReportingProfit = reportingProfit;
            ReportingCurrency = reportingCurrency;
            LastBoughtTime = lastBoughtTime;
        }

        public decimal? BtcProfit { get; }
        public decimal? ReportingProfit { get; }
        public string ReportingCurrency { get; }
        public DateTime? LastBoughtTime { get; }
        public decimal? ProfitPercentage { get; }
    }
}