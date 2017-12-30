using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Handlers.Coinigy;
using CryptoGramBot.Services.Exchanges;
using Enexure.MicroBus;

namespace CryptoGramBot.EventBus.Handlers.Binance
{
    public class BinanceQuerySymbolsCommand : ICommand
    {
    }

    public class BinanceSymbolGenerateHandler : ICommandHandler<BinanceQuerySymbolsCommand>
    {
        private readonly BinanceService _binanceService;

        public BinanceSymbolGenerateHandler(BinanceService binanceService)
        {
            _binanceService = binanceService;
        }

        public async Task Handle(BinanceQuerySymbolsCommand command)
        {
            await _binanceService.GetSymbols();
        }
    }
}