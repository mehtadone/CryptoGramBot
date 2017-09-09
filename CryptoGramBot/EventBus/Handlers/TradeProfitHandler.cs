using Enexure.MicroBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.Helpers;
using CryptoGramBot.Services;

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
            var tradesForPairAndQuantity = _databaseService.GetBuysForPairAndQuantity(query.SellReturns, query.Quantity, query.BaseCcy, query.Terms);

            if (tradesForPairAndQuantity.Count == 0)
            {
                return new TradesProfitResponse(null, null, null);
            }

            ProfitCalculator.GetProfitForTrade(tradesForPairAndQuantity, query.SellReturns, query.Quantity, out decimal? totalCost, out decimal? profit);

            if (profit.HasValue && totalCost.HasValue)
            {
                decimal? btcProfit = query.SellReturns - totalCost.Value;
                decimal? dollarProfit = await _priceService.GetDollarAmount(btcProfit.Value);
                return new TradesProfitResponse(profit, btcProfit, dollarProfit);
            }

            return new TradesProfitResponse(null, null, null);
        }
    }

    public class TradeProfitQuery : IQuery<TradeProfitQuery, TradesProfitResponse>
    {
        public TradeProfitQuery(decimal sellReturns, decimal quantity, string baseCcy, string terms)
        {
            SellReturns = sellReturns;
            Quantity = quantity;
            BaseCcy = baseCcy;
            Terms = terms;
        }

        public string BaseCcy { get; }
        public decimal Quantity { get; }
        public decimal SellReturns { get; }
        public string Terms { get; }
    }

    public class TradesProfitResponse
    {
        public TradesProfitResponse(decimal? profitPercentage, decimal? btcProfit, decimal? dollarProfit)
        {
            ProfitPercentage = profitPercentage;
            BtcProfit = btcProfit;
            DollarProfit = dollarProfit;
        }

        public decimal? BtcProfit { get; }
        public decimal? DollarProfit { get; }
        public decimal? ProfitPercentage { get; }
    }
}