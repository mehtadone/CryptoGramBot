using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.EventBus;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CryptoGramBot.Services
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
        private readonly BalanceService _balanceService;
        private readonly IMicroBus _bus;
        private readonly ILogger<PairProfitHandler> _log;

        public PairProfitHandler(BalanceService balanceService, ILogger<PairProfitHandler> log, IMicroBus bus)
        {
            _balanceService = balanceService;
            _log = log;
            _bus = bus;
        }

        public ILogger<PairProfitHandler> Log { get; }

        public async Task Handle(PairProfitCommand command)
        {
            try
            {
                var pairsArray = command.Pair.Split("-");
                var profitAndLoss = await _balanceService.GetPnLInfo(pairsArray[0], pairsArray[1]);

                var message =
                    $"{DateTime.Now:g}\n" +
                    $"Profit information for <strong>{command.Pair}</strong>\n" +
                    $"<strong>Average buy price</strong>: {profitAndLoss.AverageBuyPrice:#0.###########}\n" +
                    $"<strong>Total PnL</strong>: {profitAndLoss.Profit} BTC\n";

                await _bus.SendAsync(new SendMessageCommand(message));
            }
            catch (Exception e)
            {
                await _bus.SendAsync(new SendMessageCommand("Could not work out what the pair you typed was"));
            }
        }
    }
}