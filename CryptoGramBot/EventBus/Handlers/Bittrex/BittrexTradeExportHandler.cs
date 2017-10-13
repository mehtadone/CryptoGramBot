using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using CryptoGramBot.Services;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.EventBus.Handlers
{
    public class BittrexTradeExportCommand : ICommand
    {
        public BittrexTradeExportCommand(string fileId)
        {
            FileId = fileId;
        }

        public string FileId { get; }
    }

    public class BittrexTradeExportHandler : ICommandHandler<BittrexTradeExportCommand>
    {
        private readonly TelegramBot _bot;
        private readonly IMicroBus _bus;
        private readonly DatabaseService _databaseService;
        private readonly ILogger<BittrexTradeExportHandler> _log;

        public BittrexTradeExportHandler(TelegramBot bot, DatabaseService databaseService, IMicroBus bus, ILogger<BittrexTradeExportHandler> log)
        {
            _bot = bot;
            _databaseService = databaseService;
            _bus = bus;
            _log = log;
        }

        public async Task Handle(BittrexTradeExportCommand command)
        {
            try
            {
                var file = await _bot.Bot.GetFileAsync(command.FileId);
                var trades = TradeConverter.BittrexFileToTrades(file.FileStream, _log);
                await _databaseService.DeleteAllTrades(Constants.Bittrex);
                var newTrades = await _databaseService.AddTrades(trades);
                await _bus.SendAsync(new SendMessageCommand($"{newTrades.Count} new bittrex trades added."));
            }
            catch (Exception)
            {
                await _bus.SendAsync(new SendMessageCommand("Could not process file."));
            }
        }
    }
}