using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.Services;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;

namespace CryptoGramBot.EventBus.Handlers
{
    public class SendMessageCommand : ICommand
    {
        public SendMessageCommand(StringBuffer message)
        {
            Message = message;
        }

        public StringBuffer Message { get; }
    }

    public class SendMessageHandler : ICommandHandler<SendMessageCommand>
    {
        private readonly TelegramBot _bot;
        private readonly TelegramConfig _config;
        private readonly ILogger<SendMessageHandler> _log;

        public SendMessageHandler(TelegramBot bot, ILogger<SendMessageHandler> log, TelegramConfig config)
        {
            _bot = bot;
            _log = log;
            _config = config;
        }

        public async Task Handle(SendMessageCommand command)
        {
            var message = command.Message;

            if (message.ToString().Length <= 4094)
            {
                await _bot.SendHtmlMessage(_bot.ChatId, message.ToString(), _config.BotToken);
            }
            else
            {
                var newMessage = string.Empty;
                var strings = message.ToString().Split("\n");

                foreach (var s in strings)
                {
                    var newStringLengh = s.Length;
                    var stringWithNewLine = s + "\n";

                    if (newMessage.Length + newStringLengh <= 4094)
                    {
                        newMessage = newMessage + stringWithNewLine;
                    }
                    else if (newMessage.Length + newStringLengh == 4094)
                    {
                        newMessage = newMessage + stringWithNewLine;
                        await _bot.SendHtmlMessage(_bot.ChatId, newMessage, _config.BotToken);
                        newMessage = string.Empty;
                    }
                    else if (newMessage.Length + newStringLengh > 4094)
                    {
                        await _bot.SendHtmlMessage(_bot.ChatId, newMessage, _config.BotToken);
                        newMessage = stringWithNewLine;
                    }
                }

                if (!string.IsNullOrEmpty(newMessage))
                {
                    await _bot.SendHtmlMessage(_bot.ChatId, newMessage, _config.BotToken);
                }
            }

            _log.LogInformation($"Send Message:\n" + message);
        }
    }
}