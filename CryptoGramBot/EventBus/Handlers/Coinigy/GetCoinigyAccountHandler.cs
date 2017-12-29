using System.Threading.Tasks;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers.Coinigy
{
    public class GetCoinigyAccountCommand : ICommand
    {
    }

    public class GetCoinigyAccountHandler : ICommandHandler<GetCoinigyAccountCommand>
    {
        private readonly CoinigyApiService _coinigyApiService;

        public GetCoinigyAccountHandler(CoinigyApiService coinigyApiService)
        {
            _coinigyApiService = coinigyApiService;
        }

        public async Task Handle(GetCoinigyAccountCommand command)
        {
            await _coinigyApiService.GetAccounts();
        }
    }
}