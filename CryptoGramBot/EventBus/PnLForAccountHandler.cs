using Enexure.MicroBus;
using System.Threading.Tasks;

namespace CryptoGramBot.Services
{
    public class PnLForAccountCommand : ICommand
    {
        public PnLForAccountCommand(int accountId)
        {
            AccountId = accountId;
        }

        public int AccountId { get; }
    }

    public class PnLForAccountHandler : ICommandHandler<PnLForAccountCommand>
    {
        private readonly BalanceService _balanceService;
        private readonly IMicroBus _bus;

        public PnLForAccountHandler(BalanceService balanceService, IMicroBus bus)
        {
            _balanceService = balanceService;
            _bus = bus;
        }

        public async Task Handle(PnLForAccountCommand command)
        {
            var accountBalance24HoursAgo = await _balanceService.GetAccountBalance24HoursAgo(command.AccountId);
            await _bus.SendAsync(new BalanceUpdateCommand(accountBalance24HoursAgo.CurrentBalance,
                accountBalance24HoursAgo.PreviousBalance,
                accountBalance24HoursAgo.AccountName));
        }
    }
}