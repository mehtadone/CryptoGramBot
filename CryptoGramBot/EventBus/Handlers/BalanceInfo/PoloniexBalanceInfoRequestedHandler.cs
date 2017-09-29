using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Commands;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers.BalanceInfo
{
    public class PoloniexBalanceInfoRequestedCommand : ICommand
    {
    }

    public class PoloniexBalanceInfoRequestedHandler : ICommandHandler<PoloniexBalanceInfoRequestedCommand>
    {
        private readonly IMicroBus _bus;
        private readonly PoloniexConfig _config;
        private readonly PoloniexService _poloniexService;

        public PoloniexBalanceInfoRequestedHandler(
            IMicroBus bus,
            PoloniexService poloniexService,
            PoloniexConfig config)
        {
            _bus = bus;
            _poloniexService = poloniexService;
            _config = config;
        }

        public async Task Handle(PoloniexBalanceInfoRequestedCommand command)
        {
            var balanceInformation = await _poloniexService.GetBalance();
            await _bus.SendAsync(new SendBalanceInfoCommand(
                balanceInformation.CurrentBalance,
                balanceInformation.PreviousBalance,
                balanceInformation.WalletBalances,
                balanceInformation.AccountName
            ));
        }
    }
}