using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.EventBus;
using Enexure.MicroBus;

namespace CryptoGramBot.Services
{
    public class ExcelExportCommand : ICommand
    {
    }

    public class ExcelExportHandler : ICommandHandler<ExcelExportCommand>
    {
        private readonly BalanceService _balanceService;
        private readonly IMicroBus _bus;

        public ExcelExportHandler(BalanceService balanceService, IMicroBus bus)
        {
            _balanceService = balanceService;
            _bus = bus;
        }

        public async Task Handle(ExcelExportCommand command)
        {
            var tradeExport = _balanceService.GetTradeExport();
            await _bus.SendAsync(new SendFileCommand("TradeExport.xlsx", tradeExport.OpenRead()));
            tradeExport.Delete();
        }
    }
}