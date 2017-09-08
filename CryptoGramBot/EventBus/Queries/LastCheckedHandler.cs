using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.Database;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Queries
{
    public class LastCheckedHandler : IQueryHandler<LastCheckedQuery, LastCheckedResponse>
    {
        private readonly DatabaseService _databaseService;

        public LastCheckedHandler(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public Task<LastCheckedResponse> Handle(LastCheckedQuery query)
        {
            var dateTime = _databaseService.GetLastChecked(query.Exchange);
            return Task.FromResult(new LastCheckedResponse(dateTime));
        }
    }

    public class LastCheckedQuery : IQuery<LastCheckedQuery, LastCheckedResponse>
    {
        public LastCheckedQuery(string exchange)
        {
            Exchange = exchange;
        }

        public string Exchange { get; }
    }

    public class LastCheckedResponse
    {
        public LastCheckedResponse(DateTime lastChecked)
        {
            LastChecked = lastChecked;
        }

        public DateTime LastChecked { get; }
    }
}