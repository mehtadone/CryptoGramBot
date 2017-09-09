using System.Threading.Tasks;
using CryptoGramBot.EventBus.Commands;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers.BalanceInfo
{
    public class CoinigyPnLForAccountCommand : ICommand
    {
        public CoinigyPnLForAccountCommand(int accountId)
        {
            AccountId = accountId;
        }

        public int AccountId { get; }
    }

    public class CoinigyPnLForAccountHandler : ICommandHandler<CoinigyPnLForAccountCommand>
    {
        private readonly IMicroBus _bus;
        private readonly CoinigyBalanceService _coinigyBalanceService;

        public CoinigyPnLForAccountHandler(CoinigyBalanceService coinigyBalanceService, IMicroBus bus)
        {
            _coinigyBalanceService = coinigyBalanceService;
            _bus = bus;
        }

        public async Task Handle(CoinigyPnLForAccountCommand command)
        {
            var accountBalance24HoursAgo = await _coinigyBalanceService.GetAccountBalance(command.AccountId);
            await _bus.SendAsync(new SendBalanceInfoCommand(accountBalance24HoursAgo.CurrentBalance,
                accountBalance24HoursAgo.PreviousBalance,
                accountBalance24HoursAgo.WalletBalances,
                accountBalance24HoursAgo.AccountName));
        }
    }
}