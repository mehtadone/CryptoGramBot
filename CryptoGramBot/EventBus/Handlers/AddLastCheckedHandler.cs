using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers
{
    public class AddLastCheckedCommand : ICommand
    {
        public AddLastCheckedCommand(string exchange)
        {
            Exchange = exchange;
        }

        public string Exchange { get; }
    }

    public class AddLastCheckedHandler : ICommandHandler<AddLastCheckedCommand>
    {
        private readonly DatabaseService _databaseService;

        public AddLastCheckedHandler(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task Handle(AddLastCheckedCommand command)
        {
            await _databaseService.AddLastChecked(command.Exchange, DateTime.Now);
        }
    }
}