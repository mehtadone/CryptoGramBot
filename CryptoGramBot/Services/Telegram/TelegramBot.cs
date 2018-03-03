using System;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.Services.Telegram;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CryptoGramBot.Services
{
    public class TelegramBot
    {
        private readonly ILogger<TelegramBot> _log;
        private readonly TelegramMessageRecieveService _telegramMessageRecieveService;
        private TelegramBotClient _bot;

        public TelegramBot(TelegramMessageRecieveService telegramMessageRecieveService, ILogger<TelegramBot> log)
        {
            _telegramMessageRecieveService = telegramMessageRecieveService;
            _log = log;
        }

        public long ChatId { get; set; }

        public async Task<File> GetFileAsync(string commandFileId)
        {
            return await _bot.GetFileAsync(commandFileId);
        }

        public async Task SendDocumentAsync(long botChatId, FileToSend fileToSend)
        {
            await _bot.SendDocumentAsync(botChatId, fileToSend);
        }

        public async Task SendHtmlMessage(long botChatId, string message, string botToken)
        {
            try
            {
                await _bot.SendTextMessageAsync(botChatId, message, ParseMode.Html);
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
                ChatId = config.ChatId;
                _bot = new TelegramBotClient(config.BotToken);
                // Start the bot so we can start receiving messages
                _telegramMessageRecieveService.StartReceivingMessages(_bot);
            }
            catch (Exception ex)
            {
                _log.LogError("Could not start key. Invalid bot token\n" + ex.Message);
                throw;
            }
        }
    }
}