using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.Services;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers
{
    public class CheckCoinigyTotalBalanceCommand : ICommand
    {
    }

    public class CheckCoinigyTotalBalanceHandler : ICommandHandler<CheckCoinigyTotalBalanceCommand>
    {
        private readonly CoinigyBalanceService _coinigyBalanceService;

        public CheckCoinigyTotalBalanceHandler(CoinigyBalanceService coinigyBalanceService)
        {
            _coinigyBalanceService = coinigyBalanceService;
        }

        public async Task Handle(CheckCoinigyTotalBalanceCommand command)
        {
            await _coinigyBalanceService.GetTotalBalance();
        }
    }
}