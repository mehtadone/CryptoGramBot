using Enexure.MicroBus;
using System;
using System.Threading.Tasks;
using CryptoGramBot.Helpers;
using CryptoGramBot.Services;
using CryptoGramBot.Services.Data;
using CryptoGramBot.Services.Pricing;

namespace CryptoGramBot.EventBus.Handlers
{
    public class TradeProfitHandler : IQueryHandler<TradeProfitQuery, TradesProfitResponse>
    {
        private readonly DatabaseService _databaseService;
        private readonly PriceService _priceService;

        public TradeProfitHandler(DatabaseService databaseService, PriceService priceService)
        {
            _databaseService = databaseService;
            _priceService = priceService;
        }

        public async Task<TradesProfitResponse> Handle(TradeProfitQuery query)
        {
            var averagePrice = await _databaseService.GetBuyAveragePrice(query.BaseCcy, query.Terms, query.Exchange, query.Quantity);

            if (averagePrice == 0m)
            {
                return new TradesProfitResponse(null, null, null, null);
            }

            var totalCost = averagePrice * query.Quantity;
            var profit = ProfitCalculator.GetProfitForSell(query.SellReturns, query.Quantity, averagePrice, totalCost);

            var lastBought = await _databaseService.GetLastBoughtAsync(query.BaseCcy, query.Terms, query.Exchange);

            decimal? btcProfit = query.SellReturns - totalCost;
            decimal? dollarProfit = await _priceService.GetDollarAmount(query.BaseCcy, btcProfit.Value, query.Exchange);
            return new TradesProfitResponse(profit, btcProfit, dollarProfit, lastBought);
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
        public TradesProfitResponse(decimal? profitPercentage, decimal? btcProfit, decimal? dollarProfit, DateTime? lastBoughtTime)
        {
            ProfitPercentage = profitPercentage;
            BtcProfit = btcProfit;
            DollarProfit = dollarProfit;
            LastBoughtTime = lastBoughtTime;
        }

        public decimal? BtcProfit { get; }
        public decimal? DollarProfit { get; }
        public DateTime? LastBoughtTime { get; }
        public decimal? ProfitPercentage { get; }
    }
}