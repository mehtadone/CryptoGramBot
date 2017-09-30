using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CryptoGramBot.EventBus;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.EventBus.Handlers;
using CryptoGramBot.EventBus.Handlers.BalanceInfo;
using CryptoGramBot.Helpers;
using Enexure.MicroBus;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using MessageType = Telegram.Bot.Types.Enums.MessageType;

namespace CryptoGramBot.Services
{
    public class TelegramMessageRecieveService
    {
        private static ILogger<TelegramMessageRecieveService> _log;
        private readonly IMicroBus _bus;
        private TelegramBotClient _bot;
        private bool _waitingForFile;

        public TelegramMessageRecieveService(
            IMicroBus bus,
            ILogger<TelegramMessageRecieveService> log)
        {
            _bus = bus;
            _log = log;
        }

        public void StartReceivingMessages(TelegramBotClient bot)
        {
            _bot = bot;
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
                "Received" + e.CallbackQuery.Data);

            await CheckMessage(e.CallbackQuery.Data);
        }

        private void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs e)
        {
            _log.LogInformation($"Received choosen inline result: {e.ChosenInlineResult.ResultId}");
        }

        private async void BotOnMessageReceivedAsync(object sender, MessageEventArgs e)
        {
            var message = e.Message;

            await _bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            _log.LogInformation($"Am I waiting for the file? = {_waitingForFile}");
            if (_waitingForFile)
            {
                if (message.Document == null)
                {
                    await _bus.SendAsync(new SendMessageCommand("Did not receive a file"));
                    _waitingForFile = false;
                    return;
                }

                await _bus.SendAsync(new BittrexTradeExportCommand(message.Document.FileId));
                _waitingForFile = false;
                return;
            }

            if (message.Type != MessageType.TextMessage) return;

            try
            {
                await CheckMessage(message.Text);
            }
            catch (Exception ex)
            {
                _log.LogError($"Woops. {ex}");
                await _bus.SendAsync(new SendMessageCommand("Could not process your command. Check your logs"));
            }
        }

        private void BotOnReceiveError(object sender, ReceiveErrorEventArgs e)
        {
            _log.LogError($"Error received {e.ApiRequestException.Message}");
        }

        private async Task CheckMessage(string message)
        {
            if (message.StartsWith("/acc"))
            {
                _log.LogInformation("Message begins with /acc. Going to split string");
                var splitString = message.Split("_");

                try
                {
                    var accountNumber = splitString[1];
                    var account = int.Parse(accountNumber);

                    _log.LogInformation($"PnL check for {account}");
                    await _bus.PublishAsync(new BalanceCheckEvent(true, Constants.CoinigyAccountBalance, account));
                }
                catch (Exception)
                {
                    await SendHelpMessage();
                    _log.LogInformation($"Don't know what the user wants to do with the /acc. The message was {message}");
                }
            }
            else if (message.StartsWith(TelegramCommands.CommonPairProfit))
            {
                var splitString = message.Split(" ");
                _log.LogInformation("Profit details requested");

                try
                {
                    var pair = splitString[1];
                    _log.LogInformation($"User wants to check for profit for {pair.ToUpper()}");
                    await _bus.SendAsync(new PairProfitCommand(pair));
                }
                catch (Exception)
                {
                    await SendHelpMessage();
                    _log.LogInformation($"Don't know what the you want to do with the /profit. Format is /profit BTC-ETH for example. The message was {message}");
                }
            }
            else if (message.StartsWith(TelegramCommands.CoinigyAccountList))
            {
                try
                {
                    _log.LogInformation("PnL Account List request");
                    await _bus.SendAsync(new SendCoinigyAccountInfoCommand());
                }
                catch (Exception)
                {
                    await SendHelpMessage();
                    _log.LogInformation($"Don't know what the user wants to do with the /acc. The message was {message}");
                }
            }
            else if (message.StartsWith(TelegramCommands.CommonExcel))
            {
                _log.LogInformation("Excel sheet");
                await _bus.SendAsync(new ExcelExportCommand());
            }
            else if (message.StartsWith(TelegramCommands.CoinigyTotalBalance))
            {
                _log.LogInformation("24 Hour pnl difference for coinigy");
                await _bus.PublishAsync(new BalanceCheckEvent(true, Constants.TotalCoinigyBalance));
            }
            else if (message.StartsWith(TelegramCommands.BittrexBalanceInfo))
            {
                _log.LogInformation("24 Hour pnl difference for bittrex");
                await _bus.PublishAsync(new BalanceCheckEvent(true, Constants.Bittrex));
            }
            else if (message.StartsWith(TelegramCommands.PoloniexBalanceInfo))
            {
                _log.LogInformation("24 Hour pnl difference for poloniex");
                await _bus.PublishAsync(new BalanceCheckEvent(true, Constants.Poloniex));
            }
            else if (message.StartsWith(TelegramCommands.BittrexTradeExportUpload))
            {
                await _bus.SendAsync(new SendMessageCommand("Please upload bittrex trade export"));
                _waitingForFile = true;
            }
            else
            {
                _log.LogInformation($"Don't know what the user wants to do. The message was {message}");
                await SendHelpMessage();
            }
        }

        private async Task SendHelpMessage()
        {
            await _bus.SendAsync(new SendHelpMessageCommand());
        }
    }
}