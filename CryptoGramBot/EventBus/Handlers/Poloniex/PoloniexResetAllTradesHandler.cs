using System.Threading.Tasks;
using CryptoGramBot.Helpers;
using CryptoGramBot.Services.Data;
using CryptoGramBot.Services.Exchanges;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers.Poloniex
{
    public class PoloniexResetAllTradesHandler : ICommandHandler<ResetPoloniexTrades>
    {
        private readonly IMicroBus _bus;
        private readonly DatabaseService _databaseService;
        private readonly PoloniexService _poloniexService;

        public PoloniexResetAllTradesHandler(PoloniexService poloniexService, DatabaseService databaseService, IMicroBus bus)
        {
            _poloniexService = poloniexService;
            _databaseService = databaseService;
            _bus = bus;
        }

        public async Task Handle(ResetPoloniexTrades command)
        {
            var trades = await _poloniexService.GetOrderHistory(Constants.DateTimeUnixEpochStart);
            await _databaseService.DeleteAllTrades(Constants.Poloniex);
            await _databaseService.AddTrades(trades);
            var message = new StringBuffer();
            message.Append(StringContants.PoloniexResetTrades);
            await _bus.SendAsync(
                new SendMessageCommand(message));
        }
    }

    public class ResetPoloniexTrades : ICommand
    {
    }
}