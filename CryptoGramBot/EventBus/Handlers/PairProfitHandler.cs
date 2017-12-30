using System;
using System.Threading.Tasks;
using CryptoGramBot.Helpers;
using CryptoGramBot.Services;
using Enexure.MicroBus;

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
        private readonly ProfitAndLossService _profitAndLossService;

        public PairProfitHandler(ProfitAndLossService profitAndLossService, IMicroBus bus)
        {
            _profitAndLossService = profitAndLossService;
            _bus = bus;
        }

        public async Task Handle(PairProfitCommand command)
        {
            try
            {
                var pairsArray = command.Pair.Split("-");
                var profitAndLoss = await _profitAndLossService.GetPnLInfo(pairsArray[0], pairsArray[1], Constants.Binance);

                var sb = new StringBuffer();

                sb.Append(DateTime.Now.ToString("g") + "\n");
                sb.Append(string.Format($"Profit information for {StringContants.StrongOpen}{0}{StringContants.StrongClose}\n", command.Pair));
                sb.Append(string.Format($"{StringContants.StrongOpen}Average buy price{StringContants.StrongClose}: {0}\n", profitAndLoss.AverageBuyPrice.ToString("#0.######")));
                sb.Append(string.Format($"{StringContants.StrongOpen}Total PnL{StringContants.StrongClose}: {0} {1}", profitAndLoss.Profit.ToString("#0.######"), pairsArray[0]));

                await _bus.SendAsync(new SendMessageCommand(sb));
            }
            catch (Exception)
            {
                var er = new StringBuffer();
                er.Append(StringContants.CouldNotWorkOutPair);
                await _bus.SendAsync(new SendMessageCommand(er));
            }
        }
    }
}