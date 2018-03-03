using System.Threading.Tasks;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers
{
    public class ExcelExportCommand : ICommand
    {
    }

    public class ExcelExportHandler : ICommandHandler<ExcelExportCommand>
    {
        private readonly TelegramBot _bot;
        private readonly IMicroBus _bus;
        private readonly TradeExportService _tradeExportService;

        public ExcelExportHandler(TradeExportService tradeExportService, IMicroBus bus, TelegramBot bot)
        {
            _tradeExportService = tradeExportService;
            _bus = bus;
            _bot = bot;
        }

        public async Task Handle(ExcelExportCommand command)
        {
            var tradeExport = await _tradeExportService.GetTradeExport();
            await _bus.SendAsync(new SendFileCommand("TradeExport.xlsx", tradeExport.OpenRead(), _bot));
            tradeExport.Delete();
        }
    }
}