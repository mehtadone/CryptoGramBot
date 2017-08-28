using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Core;
using TeleCoinigy.Configuration;
using TeleCoinigy.Database;
using TeleCoinigy.Models;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TeleCoinigy.Services
{
    public class TelegramService
    {
        private static TelegramBotClient _bot;
        private static CoinigyApiService _coinigyApiService;
        private static TelegramConfig _config;
        private static DatabaseService _databaseService;
        private static Logger _log;

        public TelegramService(TelegramConfig config, CoinigyApiService coinigyApiService, DatabaseService databaseService, Logger log)
        {
            _config = config;
            _coinigyApiService = coinigyApiService;
            _databaseService = databaseService;
            _log = log;

            _bot = new TelegramBotClient(_config.BotToken);

            _bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            _bot.OnMessage += BotOnMessageReceivedAsync;
            _bot.OnMessageEdited += BotOnMessageReceivedAsync;
            _bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            _bot.OnReceiveError += BotOnReceiveError;

            var me = _bot.GetMeAsync().Result;

            Console.Title = me.Username;

            _bot.StartReceiving();
        }

        public static async Task SendAccountInfo()
        {
            var accountList = await _coinigyApiService.GetAccounts();
            var message = accountList.Aggregate("Connected accounts on Coinigy are: \n", (current, acc) => current + acc.Key + " - " + acc.Value.Name + "\n");
            _log.Information($"Sending the account list");
            await SendMessage(message);
        }

        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs e)
        {
            await _bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id,
                $"Received" + e.CallbackQuery.Data);
        }

        private static async void BotOnMessageReceivedAsync(object sender, MessageEventArgs e)
        {
            var message = e.Message;

            await _bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            if (message.Type != MessageType.TextMessage) return;

            if (message.Text.StartsWith("/acc"))
            {
                _log.Information($"Message begins with /acc. Going to split string");
                var splitString = message.Text.Split(" ");

                try
                {
                    var accountNumber = splitString[1];
                    _log.Information($"User wants to check for account number{accountNumber}");
                    SendAccountUpdate(int.Parse(accountNumber));
                }
                catch (Exception ex)
                {
                    await SendHelpMessage(message);
                    _log.Information($"Don't know what the user wants to do with the /acc. The message was {message.Text}");
                }
            }
            else if (message.Text.StartsWith("/list"))
            {
                _log.Information($"User asked for the account list");
                await SendAccountInfo();
            }
            else if (message.Text.StartsWith("/total"))
            {
                _log.Information($"User asked for the total balance");
                await SendTotalBalanace();
            }
            else
            {
                _log.Information($"Don't know what the user wants to do. The message was {message.Text}");
                await SendHelpMessage(message);
            }
        }

        private static async Task<double> GetDollarAmount(double balance)
        {
            var lastBid = await _coinigyApiService.GetTicker("BTC/USD");
            return Math.Round(lastBid * balance, 2);
        }

        private static async void SendAccountUpdate(int accountName)
        {
            var accounts = await _coinigyApiService.GetAccounts();
            var selectedAccount = accounts[accountName];
            var balance = await _coinigyApiService.GetBtcBalance(selectedAccount.AuthId);

            var dollarAmount = await GetDollarAmount(balance);

            var lastBalance = _databaseService.GetLastBalance(selectedAccount.Name);
            var currentBalance = _databaseService.AddBalance(balance, dollarAmount, selectedAccount.Name);

            _log.Information($"Sending balance update for account {selectedAccount.Name}");
            await SendBalanceUpdate(currentBalance, lastBalance, selectedAccount.Name);
        }

        private static async Task SendBalanceUpdate(BalanceHistory current, BalanceHistory lastBalance, string accountName)
        {
            var percentage = Math.Round((current.Balance - lastBalance.Balance) / lastBalance.Balance * 100, 3);
            var dollarPercentage = Math.Round(
                (current.DollarAmount - lastBalance.DollarAmount) / lastBalance.DollarAmount * 100, 3);
            var textMessage = $"{DateTime.Now:R}\n" +
                              $"<strong>Account</strong>: {accountName}\n" +
                              $"<strong>Current</strong>: {current.Balance} BTC (${current.DollarAmount})\n" +
                              $"<strong>Previous</strong>: {lastBalance.Balance} BTC (${lastBalance.DollarAmount})\n" +
                              $"<strong>Change</strong>: {percentage}% BTC ({dollarPercentage}% USD)";
            await SendMessage(textMessage);
        }

        private static async Task SendHelpMessage(Message message)
        {
            var usage = "Usage:/acc n - balance for account number\n/list - all account names\n/total - total balance";
            _log.Information($"Sending help message");
            await _bot.SendTextMessageAsync(message.Chat.Id, usage,
                replyMarkup: new ReplyKeyboardRemove());
        }

        private static async Task SendMessage(string textMessage)
        {
            await _bot.SendTextMessageAsync(_config.ChatId, textMessage, ParseMode.Html);
            _log.Information($"Message sent. Waiting for next command ...");
        }

        private static async Task SendTotalBalanace()
        {
            double balance = await _coinigyApiService.GetBtcBalance();
            var lastBalance = _databaseService.GetLastBalance(Constants.CoinigyBalance);
            var dollarAmount = await GetDollarAmount(balance);
            var currentBalance = _databaseService.AddBalance(balance, dollarAmount, Constants.CoinigyBalance);

            _log.Information($"Sending total balance");
            await SendBalanceUpdate(currentBalance, lastBalance, "Total Balance");
        }

        private void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs e)
        {
            Console.WriteLine($"Received choosen inline result: {e.ChosenInlineResult.ResultId}");
        }

        private void BotOnReceiveError(object sender, ReceiveErrorEventArgs e)
        {
            Debugger.Break();
        }
    }
}