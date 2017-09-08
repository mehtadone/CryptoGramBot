using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CryptoGramBot.Services
{
    public class TotalPnLCommand : ICommand
    {
    }

    public class TotalPnLHandler : ICommandHandler<TotalPnLCommand>
    {
        private readonly BalanceService _balanceService;
        private readonly IMicroBus _bus;
        private readonly ILogger<TotalPnLHandler> _log;

        public TotalPnLHandler(BalanceService balanceService, ILogger<TotalPnLHandler> log, IMicroBus bus)
        {
            _balanceService = balanceService;
            _log = log;
            _bus = bus;
        }

        public async Task Handle(TotalPnLCommand command)
        {
            var balance = await _balanceService.Get24HourTotalBalance();
            _log.LogInformation("Sending total pnl");
            await _bus.SendAsync(new BalanceUpdateCommand(balance.CurrentBalance, balance.PreviousBalance, "Total Balance"));
        }
    }
}