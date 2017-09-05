using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CryptoGramBot.Configuration;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CryptoGramBot.Services
{
    public class TelegramService
    {
        private static TelegramBotClient _bot;
        private static TelegramConfig _config;
        private static ILogger<TelegramService> _log;
        private readonly BalanceService _balanceService;
        private bool _waitingForFile;

        public TelegramService(
            TelegramConfig config,
            BalanceService balanceService,
            ILogger<TelegramService> log)
        {
            _config = config;

            _balanceService = balanceService;
            _log = log;
        }

        public async Task SendMessage(string textMessage, long chatId)
        {
            await _bot.SendTextMessageAsync(chatId, textMessage, ParseMode.Html);
            _log.LogInformation($"Message sent. Waiting for next command ...");
        }

        public async Task SendMessage(string textMessage)
        {
            await SendMessage(textMessage, _config.ChatId);
        }

        public async Task SendTradeNotification(Trade newTrade)
        {
            var message = $"{newTrade.TimeStamp:R}\n" +
                          $"New {newTrade.Exchange} order\n" +
                          $"<strong>{newTrade.Side} {newTrade.Base}-{newTrade.Terms}</strong>\n" +
                          $"Total: {newTrade.Cost:##0.###########} BTC\n" +
                          $"Rate: {newTrade.Limit:##0.##############} BTC";

            await SendMessage(message, _config.ChatId);
        }

        public void StartBot()
        {
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

        private async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs e)
        {
            await _bot.AnswerCallbackQueryAsync(e.CallbackQuery.Id,
                $"Received" + e.CallbackQuery.Data);

            await CheckMessage(e.CallbackQuery.Data, e.CallbackQuery.Message.Chat.Id);
        }

        private void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs e)
        {
            Console.WriteLine($"Received choosen inline result: {e.ChosenInlineResult.ResultId}");
        }

        private async void BotOnMessageReceivedAsync(object sender, MessageEventArgs e)
        {
            var message = e.Message;

            await _bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            if (_waitingForFile)
            {
                await ProcessFile(message, e.Message.Chat.Id);
                return;
            }

            if (message.Type != MessageType.TextMessage) return;

            await CheckMessage(message.Text, e.Message.Chat.Id);
        }

        private void BotOnReceiveError(object sender, ReceiveErrorEventArgs e)
        {
            _log.LogError($"Error received {e.ApiRequestException.Message}");
        }

        private async Task CheckMessage(string message, long chatId)
        {
            if (message.StartsWith("/acc"))
            {
                _log.LogInformation($"Message begins with /acc. Going to split string");
                var splitString = message.Split("_");

                try
                {
                    if (splitString.Length == 2)
                    {
                        var accountNumber = splitString[1];
                        _log.LogInformation($"Balance check for {accountNumber}");
                        SendAccountUpdate(int.Parse(accountNumber), chatId);
                    }
                    else if (splitString.Length == 3 && splitString[2] == "pnl")
                    {
                        var accountNumber = splitString[1];
                        _log.LogInformation($"pPnL check for {accountNumber}");
                        await SendPnlUpdateForAccountNumber(int.Parse(accountNumber), chatId);
                    }
                }
                catch (Exception)
                {
                    await SendHelpMessage();
                    _log.LogInformation($"Don't know what the user wants to do with the /acc. The message was {message}");
                }
            }
            else if (message.StartsWith("/profit"))
            {
                var splitString = message.Split(" ");
                _log.LogInformation($"Profit details requested");

                try
                {
                    var pair = splitString[1];
                    var ccys = pair.Split('-');
                    _log.LogInformation($"User wants to check for profit for {pair}");
                    await SendProfitInfomation(chatId, ccys[0], ccys[1]);
                }
                catch (Exception)
                {
                    await SendHelpMessage();
                    _log.LogInformation($"Don't know what the user wants to do with the /profit. The message was {message}");
                }
            }
            else if (message.StartsWith("/list"))
            {
                _log.LogInformation($"Message begins with /acc. Going to split string");
                var splitString = message.Split("_");

                try
                {
                    if (splitString.Length == 1)
                    {
                        _log.LogInformation($"Account list request");
                        await SendAccountInfo(chatId);
                    }
                    else if (splitString.Length == 2 && splitString[1] == "pnl")
                    {
                        _log.LogInformation($"PnL Account List request");
                        await SendPnLAccountInfo(chatId);
                    }
                }
                catch (Exception)
                {
                    await SendHelpMessage();
                    _log.LogInformation($"Don't know what the user wants to do with the /acc. The message was {message}");
                }
            }
            else if (message.StartsWith("/excel"))
            {
                _log.LogInformation($"Excel sheet");
                await SendAccountInfo(chatId);
            }
            else if (message.StartsWith("/total"))
            {
                var strings = message.Split('_');
                if (strings.Length == 1)
                {
                    _log.LogInformation($"Total balance request");
                    await SendTotalBalance(chatId);
                }
                else if (strings.Length == 2 && strings[1] == "pnl")
                {
                    _log.LogInformation($"24 Hour pnl difference");
                    await SendTotalPnL(chatId);
                }
            }
            else if (message.StartsWith("/total_pnl"))
            {
            }
            else if (message.StartsWith("/upload_bittrex_orders"))
            {
                await SendMessage("Please upload bittrex trade export", chatId);
                _waitingForFile = true;
            }
            else
            {
                _log.LogInformation($"Don't know what the user wants to do. The message was {message}");
                await SendHelpMessage();
            }
        }

        private async Task ProcessFile(Message message, long chatId)
        {
            if (message.Document == null)
            {
                await SendMessage("Did not receive a file", chatId);
                _waitingForFile = false;
                return;
            }

            try
            {
                var file = await _bot.GetFileAsync(message.Document.FileId);
                var trades = TradeConverter.BittrexFileToTrades(file.FileStream);
                _balanceService.AddTrades(trades, out List<Trade> newTrades);
                await SendMessage($"{newTrades.Count} new bittrex trades added.", chatId);
                _waitingForFile = false;
            }
            catch (Exception)
            {
                await SendMessage("Could not process file.", chatId);
                _waitingForFile = false;
            }
        }

        private async Task SendAccountInfo(long chatId)
        {
            var accountList = await _balanceService.GetAccounts();
            var message = accountList.Aggregate($"{DateTime.Now:R}\n" + "Connected accounts on Coinigy are: \n", (current, acc) => current + "/acc_" + acc.Key + " - " + acc.Value.Name + "\n");
            _log.LogInformation($"Sending the account list");
            await SendMessage(message, chatId);
        }

        private async void SendAccountUpdate(int accountId, long chatId)
        {
            var balance = await _balanceService.GetAccountBalance(accountId);
            _log.LogInformation($"Sending balance update for account {accountId}");
            await SendBalanceUpdate($"Since {balance.PreviousBalance.DateTime:R}", balance.CurrentBalance, balance.PreviousBalance, balance.AccountName, chatId);
        }

        private async Task SendBalanceUpdate(string preMessage, BalanceHistory current, BalanceHistory lastBalance, string accountName, long chatId)
        {
            var message = preMessage + "\n" +
                             $"{DateTime.Now:R}\n" +
                             $"<strong>Account</strong>: {accountName}\n" +
                             $"<strong>Current</strong>: {current.Balance} BTC (${current.DollarAmount})\n" +
                             $"<strong>Previous</strong>: {lastBalance.Balance} BTC (${lastBalance.DollarAmount})\n" +
                             $"<strong>Difference</strong>: {(current.Balance - lastBalance.Balance):##0.###########} BTC (${Math.Round(current.DollarAmount - lastBalance.DollarAmount, 2)})\n";

            try
            {
                var percentage = Math.Round((current.Balance - lastBalance.Balance) / lastBalance.Balance * 100, 2);

                var dollarPercentage = Math.Round(
                    (current.DollarAmount - lastBalance.DollarAmount) / lastBalance.DollarAmount * 100, 2);

                message = message + $"<strong>Change</strong>: {percentage}% BTC ({dollarPercentage}% USD)";
            }
            catch (Exception e)
            {
                await SendMessage("Could not calculate percentages");
            }
            await SendMessage(message, chatId);
        }

        private async Task SendHelpMessage()
        {
            var usage = $"Usage:\n" +
                        "/acc_n - balance for account number n.\n" +
                        "/acc_n_pnl - 24 hour pnl for account number n.\n" +
                        "/list - all account names\n" +
                        "/list_pnl - all account names for pnl\n" +
                        "/total - total balance\n" +
                        "/total_pnl - 24 hour PnL for the total balance\n" +
                        "/excel - an excel export of all trades\n" +
                        "/profit BTC-XXX - profit information for pair\n" +
                        "/upload_bittrex_orders - upload bittrex order export";
            _log.LogInformation($"Sending help message");
            await _bot.SendTextMessageAsync(_config.ChatId, usage,
                replyMarkup: new ReplyKeyboardRemove());
        }

        private async Task SendPnLAccountInfo(long chatId)
        {
            var accountList = await _balanceService.GetAccounts();
            var message = accountList.Aggregate($"{DateTime.Now:R}\n" + "Connected accounts on Coinigy are: \n", (current, acc) => current + "/acc_" + acc.Key + "_pnl " + " - " + acc.Value.Name + "\n");
            _log.LogInformation($"Sending the account list with pnl");
            await SendMessage(message, chatId);
        }

        private async Task SendPnlUpdateForAccountNumber(int accountId, long chatId)
        {
            var accountBalance24HoursAgo = await _balanceService.GetAccountBalance24HoursAgo(accountId);
            await SendBalanceUpdate("24 Hour Summary", accountBalance24HoursAgo.CurrentBalance, accountBalance24HoursAgo.PreviousBalance,
                accountBalance24HoursAgo.AccountName, chatId);
        }

        private async Task SendProfitInfomation(long chatId, string ccy1, string ccy2)
        {
            var profitAndLoss = await _balanceService.GetPnLInfo(ccy1, ccy2);

            var message =
                $"{DateTime.Now:R}\n" +
                $"Profit information for <strong>{ccy1 + "-" + ccy2}</strong>\n" +
                             $"<strong>Average buy price</strong>: {profitAndLoss.AverageBuyPrice:#0.###########}\n" +
                             $"<strong>Total PnL</strong>: {profitAndLoss.Profit} BTC\n";

            await SendMessage(message, chatId);
        }

        private async Task SendTotalBalance(long chatId)
        {
            var balance = await _balanceService.GetTotalBalance();
            _log.LogInformation($"Sending total balance");
            await SendBalanceUpdate($"Since {balance.PreviousBalance.DateTime:R}", balance.CurrentBalance, balance.PreviousBalance, "Total Balance", chatId);
        }

        private async Task SendTotalPnL(long chatId)
        {
            var balance = await _balanceService.Get24HourTotalBalance();
            _log.LogInformation($"Sending total pnl");
            await SendBalanceUpdate($"24 hour summary", balance.CurrentBalance, balance.PreviousBalance, "Total Balance", chatId);
        }
    }
}