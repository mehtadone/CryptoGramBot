using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.Services;
using Enexure.MicroBus;
using Telegram.Bot;
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
        private readonly TelegramConfig _config;

        public SendFileHandler(TelegramBot bot, TelegramConfig config)
        {
            _bot = bot;
            _config = config;
        }

        public async Task Handle(SendFileCommand command)
        {
            var bot = new TelegramBotClient(_config.BotToken);
            await bot.SendDocumentAsync(_bot.ChatId, new FileToSend(command.FileName, command.Stream));
        }
    }
}