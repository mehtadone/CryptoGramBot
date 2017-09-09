using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Commands;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers.BalanceInfo
{
    public class BittrexBalanceInfoRequestedCommand : ICommand
    {
    }

    public class BittrexBalanceInfoRequestedHandler : ICommandHandler<BittrexBalanceInfoRequestedCommand>
    {
        private readonly BittrexService _bittrexService;
        private readonly IMicroBus _bus;
        private readonly BittrexConfig _config;

        public BittrexBalanceInfoRequestedHandler(
            IMicroBus bus,
            BittrexService bittrexService,
            BittrexConfig config)
        {
            _bus = bus;
            _bittrexService = bittrexService;
            _config = config;
        }

        public async Task Handle(BittrexBalanceInfoRequestedCommand command)
        {
            var balanceInformation = await _bittrexService.GetBalance();
            await _bus.SendAsync(new SendBalanceInfoCommand(
                balanceInformation.CurrentBalance,
                balanceInformation.PreviousBalance,
                balanceInformation.WalletBalances,
                balanceInformation.AccountName
                ));
        }
    }
}