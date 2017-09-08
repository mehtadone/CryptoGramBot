using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using Telegram.Bot;

namespace CryptoGramBot.Services
{
    public class TelegramBot
    {
        private readonly TelegramMessageRecieveService _telegramMessageRecieveService;

        public TelegramBot(TelegramMessageRecieveService telegramMessageRecieveService)
        {
            _telegramMessageRecieveService = telegramMessageRecieveService;
        }

        public TelegramBotClient Bot { get; set; }

        public long ChatId { get; set; }

        public void StartBot(TelegramConfig config)
        {
            Bot = new TelegramBotClient(config.BotToken);
            ChatId = config.ChatId;

            // Start the bot so we can start receiving messages
            _telegramMessageRecieveService.StartReceivingMessages(Bot);
        }
    }
}