using System.Threading.Tasks;
using CryptoGramBot.EventBus.Handlers;
using CryptoGramBot.EventBus.Handlers.Bittrex;
using CryptoGramBot.Helpers;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace CryptoGramBot.Services.Telegram
{
    public class TelegramBittrexFileUploadService
    {
        private readonly IMicroBus _bus;
        private readonly ILogger<TelegramBittrexFileUploadService> _log;

        public TelegramBittrexFileUploadService(IMicroBus bus, ILogger<TelegramBittrexFileUploadService> log)
        {
            _bus = bus;
            _log = log;
        }

        public async Task<bool> AreWeFileHandling(Document document)
        {
            if (!BittrexFileUploadState.Waiting) return false;

            _log.LogInformation($"Am I waiting for the file? = {BittrexFileUploadState.Waiting}");
            if (document == null)
            {
                var message = new StringBuffer();
                message.Append(StringContants.DidNotRecieveFile);
                await _bus.SendAsync(new SendMessageCommand(message));
                BittrexFileUploadState.Reset();
                return true;
            }

            await _bus.SendAsync(new BittrexTradeExportCommand(document.FileId));
            BittrexFileUploadState.Reset();
            return true;
        }
    }
}