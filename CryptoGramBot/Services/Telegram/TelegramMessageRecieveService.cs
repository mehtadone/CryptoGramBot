using System;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.EventBus.Handlers;
using CryptoGramBot.EventBus.Handlers.Poloniex;
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
        private readonly TelegramConfig _config;
        private readonly TelegramBittrexFileUploadService _fileImportService;
        private readonly TelegramPairProfitService _pairProfitService;
        private readonly TelegramMessageSendingService _sendingService;
        private TelegramBotClient _bot;

        public TelegramMessageRecieveService(
            IMicroBus bus,
            TelegramMessageSendingService sendingService,
            TelegramBittrexFileUploadService fileImportService,
            TelegramPairProfitService pairProfitService,
            TelegramConfig config,
            ILogger<TelegramMessageRecieveService> log)
        {
            _bus = bus;
            _sendingService = sendingService;
            _fileImportService = fileImportService;
            _pairProfitService = pairProfitService;
            _config = config;
            _log = log;
        }

        public void StartReceivingMessages(TelegramBotClient bot)
        {
            SetupBot(bot);
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

            if (await CheckStates(message)) return;

            if (message.Type != MessageType.TextMessage) return;

            try
            {
                await CheckMessage(message.Text);
            }
            catch (Exception ex)
            {
                _log.LogError($"Woops. {ex}");
                var sb = new StringBuffer();
                sb.Append(StringContants.CouldNotProcessCommand);
                await _bus.SendAsync(new SendMessageCommand(sb));
            }
        }

        private void BotOnReceiveError(object sender, ReceiveErrorEventArgs e)
        {
            _log.LogError($"Error received {e.ApiRequestException.Message}");

            _bot.OnCallbackQuery -= BotOnCallbackQueryReceived;
            _bot.OnMessage -= BotOnMessageReceivedAsync;
            _bot.OnMessageEdited -= BotOnMessageReceivedAsync;
            _bot.OnInlineResultChosen -= BotOnChosenInlineResultReceived;
            _bot.OnReceiveError -= BotOnReceiveError;

            _bot.StopReceiving();

            SetupBot(_bot);
        }

        private async Task CheckMessage(string message)
        {
            if (message.StartsWith("/acc"))
            {
                await _sendingService.CoinigyAccountBalance(message);
            }
            else if (message.StartsWith(TelegramCommands.CommonPairProfit))
            {
                await _pairProfitService.RequestedPairProfit();
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
            else if (message.StartsWith(TelegramCommands.BinanceBalanceInfo))
            {
                await _sendingService.BinanceBalance();
            }
            else if (message.StartsWith(TelegramCommands.PoloniexBalanceInfo))
            {
                await _sendingService.PoloniexBalance();
            }
            else if (message.StartsWith(TelegramCommands.BittrexTradeExportUpload))
            {
                await _sendingService.BittrexTradeImport();
            }
            else if (message.StartsWith(TelegramCommands.PoloniexTradeReset))
            {
                await _bus.SendAsync(new ResetPoloniexTrades());
            }
            else
            {
                _log.LogInformation($"Don't know what the user wants to do. The message was {message}");
                await _sendingService.SendHelpMessage();
            }
        }

        private async Task<bool> CheckStates(Message message)
        {
            if (await _fileImportService.AreWeFileHandling(message.Document)) return true;

            if (await _pairProfitService.SendPairProfit(message.Text)) return true;

            return false;
        }

        private void SetupBot(TelegramBotClient bot)
        {
            _bot = bot;
            _bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            _bot.OnMessage += BotOnMessageReceivedAsync;
            _bot.OnMessageEdited += BotOnMessageReceivedAsync;
            _bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            _bot.OnReceiveError += BotOnReceiveError;

            _bot.StartReceiving();
        }
    }
}