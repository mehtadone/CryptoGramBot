using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.EventBus.Handlers;
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
                await _bus.SendAsync(new SendMessageCommand(new StringBuilder("Did not receive a file")));
                BittrexFileUploadState.Reset();
                return true;
            }

            await _bus.SendAsync(new BittrexTradeExportCommand(document.FileId));
            BittrexFileUploadState.Reset();
            return true;
        }
    }
}