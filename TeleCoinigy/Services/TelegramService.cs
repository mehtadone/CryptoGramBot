using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleCoinigy.Configuration;
using TeleCoinigy.Models;
using Telegram.Bot;

namespace TeleCoinigy.Services
{
    public class TelegramService
    {
        private readonly TelegramBotClient _bot;
        private readonly TelegramConfig _config;

        public TelegramService(TelegramConfig config)
        {
            _config = config;
            _bot = new TelegramBotClient(_config.BotToken);
        }

        public async Task SendAccountInfo(List<Account> accounts)
        {
            var message = accounts.Aggregate("Connected accounts on Coinigy are: \n", (current, acc) => current + acc.Name + "\n");
            await SendMessage(message);
        }

        public async Task SendBalanceUpdate(double balance, double lastBalance)
        {
            await SendBalanceUpdate(balance, lastBalance, "All Accounts");
        }

        public async Task SendBalanceUpdate(double balance, double lastBalance, string accountName)
        {
            var percentage = (lastBalance - balance) / lastBalance * 100;
            var textMessage = "Account: " + accountName + "\n" + DateTime.Now.ToString("g") + "\nCurrent balance is           " + balance + " BTC\nPrevious balance was    " + lastBalance + " BTC\nPercentage change is " + percentage + "%";
            await SendMessage(textMessage);
        }

        private async Task SendMessage(string textMessage)
        {
            await _bot.SendTextMessageAsync(_config.ChatId, textMessage);
        }
    }
}