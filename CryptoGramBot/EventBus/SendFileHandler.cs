using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.Services;
using Enexure.MicroBus;
using Telegram.Bot.Types;

namespace CryptoGramBot.EventBus
{
    public class SendFileCommand : ICommand
    {
        public SendFileCommand(string fileName, Stream stream)
        {
            FileName = fileName;
            Stream = stream;
        }

        public string FileName { get; }
        public Stream Stream { get; }
    }

    public class SendFileHandler : ICommandHandler<SendFileCommand>
    {
        private readonly TelegramBot _bot;

        public SendFileHandler(TelegramBot bot)
        {
            _bot = bot;
        }

        public async Task Handle(SendFileCommand command)
        {
            await _bot.Bot.SendDocumentAsync(_bot.ChatId, new FileToSend(command.FileName, command.Stream));
        }
    }
}