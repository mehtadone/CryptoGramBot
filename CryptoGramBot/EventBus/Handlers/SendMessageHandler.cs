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
            string message = command.Message;
            await _bot.SendHtmlMessage(_bot.ChatId, message);

            //            if (message.Length <= 4096)
            //            {
            //                await _bot.SendHtmlMessage(_bot.ChatId, command.Message);
            //            }
            //            else
            //            {
            //                string newMessage = String.Empty;
            //                var strings = message.Split("\n");
            //
            //                foreach (var s in strings)
            //                {
            //                    if (newMessage.Length <= 4096)
            //                    {
            //                        ne
            //                    }
            //                }
            //
            //            }

            _log.LogInformation($"Send Message:\n" + command.Message);
        }
    }
}