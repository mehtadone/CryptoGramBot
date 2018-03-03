using System;
using System.Threading.Tasks;
using CryptoGramBot.Helpers;
using CryptoGramBot.Services;
using CryptoGramBot.Services.Data;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace CryptoGramBot.EventBus.Handlers.Bittrex
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

        public BittrexTradeExportHandler(DatabaseService databaseService, IMicroBus bus, ILogger<BittrexTradeExportHandler> log, TelegramBot bot)
        {
            _databaseService = databaseService;
            _bus = bus;
            _log = log;
            _bot = bot;
        }

        public async Task Handle(BittrexTradeExportCommand command)
        {
            try
            {
                var file = await _bot.GetFileAsync(command.FileId);
                var trades = BittrexConvertor.BittrexFileToTrades(file.FileStream, _log);
                await _databaseService.DeleteAllTrades(Constants.Bittrex);
                var newTrades = await _databaseService.AddTrades(trades);

                var sb = new StringBuffer();
                sb.Append(string.Format("{0} new bittrex trades added.", newTrades.Count));

                await _bus.SendAsync(new SendMessageCommand(sb));
            }
            catch (Exception)
            {
                var sb = new StringBuffer();
                sb.Append(StringContants.CouldNotProcessFile);
                await _bus.SendAsync(new SendMessageCommand(sb));
            }
        }
    }
}