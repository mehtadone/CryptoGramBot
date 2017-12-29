using System;
using System.Threading.Tasks;
using CryptoGramBot.Services;
using CryptoGramBot.Services.Data;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers
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