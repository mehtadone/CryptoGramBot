using System;
using System.Threading.Tasks;
using CryptoGramBot.Services;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;
using Serilog;
using Telegram.Bot.Types.Enums;

namespace CryptoGramBot.EventBus.Handlers
{
    public class SendMessageCommand : ICommand
    {
        public SendMessageCommand(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }

    public class SendMessageHandler : ICommandHandler<SendMessageCommand>
    {
        private readonly TelegramBot _bot;
        private readonly ILogger<SendMessageHandler> _log;

        public SendMessageHandler(TelegramBot bot, ILogger<SendMessageHandler> log)
        {
            _bot = bot;
            _log = log;
        }

        public async Task Handle(SendMessageCommand command)
        {
            var message = command.Message;

            //            message = String.Empty;
            //            int x = 0;
            //            while (x <= 4500)
            //            {
            //                message = message + $"I am test message {x}\n";
            //                x++;
            //            }

            if (message.Length <= 4096)
            {
                await _bot.SendHtmlMessage(_bot.ChatId, message);
            }
            else
            {
                var newMessage = string.Empty;
                var strings = message.Split("\n");

                foreach (var s in strings)
                {
                    var newStringLengh = s.Length;
                    if (newMessage.Length + newStringLengh <= 4096)
                    {
                        newMessage = newMessage + s;
                    }
                    else if (newMessage.Length + newStringLengh == 4096)
                    {
                        newMessage = newMessage + s;
                        await _bot.SendHtmlMessage(_bot.ChatId, newMessage);
                        newMessage = string.Empty;
                    }
                    else if (newMessage.Length + newStringLengh > 4096)
                    {
                        await _bot.SendHtmlMessage(_bot.ChatId, newMessage);
                        newMessage = s;
                    }
                }
            }

            _log.LogInformation($"Send Message:\n" + message);
        }
    }
}