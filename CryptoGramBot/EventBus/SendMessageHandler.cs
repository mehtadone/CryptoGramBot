using System.Threading.Tasks;
using CryptoGramBot.Services;
using Enexure.MicroBus;
using Telegram.Bot.Types.Enums;

namespace CryptoGramBot.EventBus
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

        public SendMessageHandler(TelegramBot bot)
        {
            _bot = bot;
        }

        public async Task Handle(SendMessageCommand command)
        {
            await _bot.Bot.SendTextMessageAsync(_bot.ChatId, command.Message, ParseMode.Html);
        }
    }
}