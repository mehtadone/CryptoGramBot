using System;
using System.Threading.Tasks;
using CryptoGramBot.EventBus.Handlers;
using CryptoGramBot.Helpers;
using Enexure.MicroBus;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using MessageType = Telegram.Bot.Types.Enums.MessageType;

namespace CryptoGramBot.Services.Telegram
{
    public class TelegramMessageRecieveService
    {
        private static ILogger<TelegramMessageRecieveService> _log;
        private readonly IMicroBus _bus;
        private readonly TelegramMessageSendingService _sendingService;
        private TelegramBotClient _bot;
        private bool _waitingForFile;

        public TelegramMessageRecieveService(
            IMicroBus bus,
            TelegramMessageSendingService sendingService,
            ILogger<TelegramMessageRecieveService> log)
        {
            _bus = bus;
            _sendingService = sendingService;
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

        private async Task<bool> AreWeFileHandling(Message message)
        {
            if (!_waitingForFile) return false;
            _log.LogInformation($"Am I waiting for the file? = {_waitingForFile}");
            if (message.Document == null)
            {
                await _bus.SendAsync(new SendMessageCommand("Did not receive a file"));
                Reset();
                return true;
            }

            await _bus.SendAsync(new BittrexTradeExportCommand(message.Document.FileId));
            Reset();
            return true;
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

            if (await AreWeFileHandling(message)) return;

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
                await _sendingService.CoinigyAccountBalance(message);
            }
            else if (message.StartsWith(TelegramCommands.CommonPairProfit))
            {
                await _sendingService.PairProfit(message);
            }
            else if (message.StartsWith(TelegramCommands.CoinigyAccountList))
            {
                await _sendingService.CoinigyAccountList(message);
            }
            else if (message.StartsWith(TelegramCommands.CommonExcel))
            {
                await _sendingService.ExcelSheet();
            }
            else if (message.StartsWith(TelegramCommands.CoinigyTotalBalance))
            {
                await _sendingService.TotalCoinigyBalance();
            }
            else if (message.StartsWith(TelegramCommands.BittrexBalanceInfo))
            {
                await _sendingService.BittrexBalance();
            }
            else if (message.StartsWith(TelegramCommands.PoloniexBalanceInfo))
            {
                await _sendingService.PoloniexBalance();
            }
            else if (message.StartsWith(TelegramCommands.BittrexTradeExportUpload))
            {
                await _sendingService.BittrexTradeImport();
                _waitingForFile = true;
            }
            else
            {
                _log.LogInformation($"Don't know what the user wants to do. The message was {message}");
                await _sendingService.SendHelpMessage();
            }
        }

        private void Reset()
        {
            _waitingForFile = false;
        }
    }
}