using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoGramBot.Models;
using CryptoGramBot.Services;
using CryptoGramBot.Services.Data;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers
{
    public class FindNewTradeQuery : IQuery<FindNewTradeQuery, FindNewTradesResponse>
    {
        public FindNewTradeQuery(IEnumerable<Trade> orderHistory)
        {
            OrderHistory = orderHistory;
        }

        public IEnumerable<Trade> OrderHistory { get; }
    }

    public class FindNewTradesResponse
    {
        public FindNewTradesResponse(IOrderedEnumerable<Trade> newTrades)
        {
            NewTrades = newTrades;
        }

        public IOrderedEnumerable<Trade> NewTrades { get; }
    }

    public class SaveAndFindNewTradesHandler : IQueryHandler<FindNewTradeQuery, FindNewTradesResponse>
    {
        private readonly DatabaseService _databaseService;

        public SaveAndFindNewTradesHandler(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<FindNewTradesResponse> Handle(FindNewTradeQuery query)
        {
            var newTrades = await _databaseService.AddTrades(query.OrderHistory);
            IOrderedEnumerable<Trade> orderedEnumerable = newTrades.OrderBy(x => x.TimeStamp);
            return new FindNewTradesResponse(orderedEnumerable);
        }
    }
}