using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.Services.Telegram;
using Microsoft.Extensions.Logging;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace CryptoGramBot.Services
{
    public class TelegramBot
    {
        private readonly ILogger<TelegramBot> _log;
        private readonly TelegramMessageRecieveService _telegramMessageRecieveService;

        public TelegramBot(TelegramMessageRecieveService telegramMessageRecieveService, ILogger<TelegramBot> log)
        {
            _telegramMessageRecieveService = telegramMessageRecieveService;
            _log = log;
        }

        public TelegramBotClient Bot { get; set; }

        public long ChatId { get; set; }

        public async Task SendHtmlMessage(long botChatId, string message)
        {
            try
            {
                await Bot.SendTextMessageAsync(botChatId, message, ParseMode.Html);
            }
            catch (Exception ex)
            {
                _log.LogError("Could not send message. Invalid chat id\n" + ex.Message);
                throw;
            }
        }

        public void StartBot(TelegramConfig config)
        {
            try
            {
                Bot = new TelegramBotClient(config.BotToken);
                ChatId = config.ChatId;
            }
            catch (Exception ex)
            {
                _log.LogError("Could not start key. Invalid bot token\n" + ex.Message);
                throw;
            }

            // Start the bot so we can start receiving messages
            _telegramMessageRecieveService.StartReceivingMessages(Bot);
        }
    }
}