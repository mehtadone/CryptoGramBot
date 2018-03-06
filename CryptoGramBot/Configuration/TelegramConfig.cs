using Microsoft.Extensions.Logging;
using CryptoGramBot.Helpers;

namespace CryptoGramBot.Configuration
{
    public class TelegramConfig
    {
        private readonly ILogger<TelegramConfig> _log;

        public string BotToken { get; set; }
        public long ChatId { get; set; }

        public TelegramConfig(ILogger<TelegramConfig> log)
        {
            _log = log;
        }

        public bool IsValid()
        {
            bool result = true;

            if (string.IsNullOrEmpty(BotToken) || (BotToken == Constants.ConfigDummyValue))
            {
                result = false;
                _log.LogError($"BotToken is invalid or missing in Telegram config");
            }

            if (ChatId == 0)
            {
                result = false;
                _log.LogError($"ChatId is invalid or missing in Telegram config");
            }

            return result;
        }
    }
}