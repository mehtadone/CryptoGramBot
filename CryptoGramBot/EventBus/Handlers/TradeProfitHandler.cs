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

        public TradeProfitHandler(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public Task<TradesProfitResponse> Handle(TradeProfitQuery query)
        {
            var tradesForPairAndQuantity = _databaseService.GetBuysForPairAndQuantity(query.SellReturns, query.Quantity, query.BaseCcy, query.Terms);

            if (tradesForPairAndQuantity.Count == 0)
            {
                return Task.FromResult(new TradesProfitResponse(null));
            }

            return Task.FromResult(new TradesProfitResponse(ProfitCalculator.GetProfitForTrade(tradesForPairAndQuantity, query.SellReturns, query.Quantity)));
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
        public TradesProfitResponse(decimal? profitPercentage)
        {
            ProfitPercentage = profitPercentage;
        }

        public decimal? ProfitPercentage { get; }
    }
}