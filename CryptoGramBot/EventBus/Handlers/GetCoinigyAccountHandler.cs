using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers
{
    public class GetCoinigyAccountCommand : ICommand
    {
    }

    public class GetCoinigyAccountHandler : ICommandHandler<GetCoinigyAccountCommand>
    {
        private readonly CoinigyApiService _coinigyApiService;
        private readonly DatabaseService _databaseService;

        public GetCoinigyAccountHandler(DatabaseService databaseService, CoinigyApiService coinigyApiService)
        {
            _databaseService = databaseService;
            _coinigyApiService = coinigyApiService;
        }

        public async Task Handle(GetCoinigyAccountCommand command)
        {
            var coinigyAccounts = await _coinigyApiService.GetAccounts();
            _databaseService.AddCoinigyAccounts(coinigyAccounts);
        }
    }
}