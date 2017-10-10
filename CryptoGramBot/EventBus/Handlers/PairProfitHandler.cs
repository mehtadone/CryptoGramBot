using System;
using System.Threading.Tasks;
using CryptoGramBot.Services;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.EventBus.Handlers
{
    public class PairProfitCommand : ICommand
    {
        public PairProfitCommand(string pair)
        {
            Pair = pair;
        }

        public string Pair { get; }
    }

    public class PairProfitHandler : ICommandHandler<PairProfitCommand>
    {
        private readonly IMicroBus _bus;
        private readonly ILogger<PairProfitHandler> _log;
        private readonly ProfitAndLossService _profitAndLossService;

        public PairProfitHandler(ProfitAndLossService profitAndLossService, ILogger<PairProfitHandler> log, IMicroBus bus)
        {
            _profitAndLossService = profitAndLossService;
            _log = log;
            _bus = bus;
        }

        public ILogger<PairProfitHandler> Log { get; }

        public async Task Handle(PairProfitCommand command)
        {
            try
            {
                var pairsArray = command.Pair.Split("-");
                var profitAndLoss = await _profitAndLossService.GetPnLInfo(pairsArray[0], pairsArray[1]);

                var message =
                    $"{DateTime.Now:g}\n" +
                    $"Profit information for <strong>{command.Pair}</strong>\n" +
                    $"<strong>Average buy price</strong>: {profitAndLoss.AverageBuyPrice:#0.###########}\n" +
                    $"<strong>Total PnL</strong>: {profitAndLoss.Profit} {pairsArray[0]}\n";

                await _bus.SendAsync(new SendMessageCommand(message));
            }
            catch (Exception)
            {
                await _bus.SendAsync(new SendMessageCommand("Could not work out what the pair you typed was"));
            }
        }
    }
}