using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using FluentScheduler;
using CryptoGramBot.Database;
using CryptoGramBot.EventBus;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using Enexure.MicroBus;

namespace CryptoGramBot.Services
{
    public class StartupService
    {
        private readonly BalanceService _balanceService;
        private readonly BittrexService _bittrexService;
        private readonly TelegramBot _bot;
        private readonly IMicroBus _bus;
        private readonly DatabaseService _databaseService;
        private readonly PoloniexService _poloniexService;
        private readonly TelegramConfig _telegramConfig;
        private readonly TelegramMessageRecieveService _telegramMessageRecieveService;

        public StartupService(
            IMicroBus bus,
            TelegramConfig telegramConfig,
            TelegramMessageRecieveService telegramMessageRecieveService,
            BittrexService bittrexService,
            PoloniexService poloniexService,
            DatabaseService databaseService,
            BalanceService balanceService,
            TelegramBot bot
            )
        {
            _bus = bus;
            _telegramConfig = telegramConfig;
            _telegramMessageRecieveService = telegramMessageRecieveService;
            _bittrexService = bittrexService;
            _poloniexService = poloniexService;
            _databaseService = databaseService;
            _balanceService = balanceService;
            _bot = bot;
        }

        public async Task CheckCoinigyBalances()
        {
            await _balanceService.GetAllBalances();
            await _balanceService.GetTotalBalance();
        }

        public async Task GetNewOrders()
        {
            var newOrdersFromBittrex = GetNewOrdersFromBittrex();

            foreach (var newTrade in newOrdersFromBittrex)
            {
                await _bus.SendAsync(new TradeNotificationCommand(newTrade));
            }

            var newOrdersFromPolo = GetNewOrdersFromPolo();

            foreach (var newTrade in newOrdersFromPolo)
            {
                await _bus.SendAsync(new TradeNotificationCommand(newTrade));
            }
        }

        public async Task GetNewOrdersOnStartup()
        {
            var message = "<strong>Checking new orders on startup. Will only send top 5</strong>\n";
            await _bus.SendAsync(new SendMessageCommand(message));

            var newTradesBittrex = GetNewOrdersFromBittrex();
            await SendNewTradeNotificationsOnStartup(newTradesBittrex);

            var newTradesPolo = GetNewOrdersFromPolo();
            await SendNewTradeNotificationsOnStartup(newTradesPolo);
        }

        public void Start()
        {
            _bot.StartBot(_telegramConfig);
            _telegramMessageRecieveService.StartBot(_bot.Bot);

            var registry = new Registry();
            registry.Schedule(() => GetNewOrdersOnStartup().Wait()).ToRunNow();
            registry.Schedule(() => GetNewOrders().Wait()).ToRunEvery(5).Minutes();
            registry.Schedule(() => CheckCoinigyBalances().Wait()).ToRunNow().AndEvery(1).Hours().At(0);
            registry.Schedule(() => CheckForBags().Wait()).ToRunNow().AndEvery(6).Hours();

            JobManager.Initialize(registry);
        }

        private async Task CheckForBags()
        {
            var walletBalances = _bittrexService.GetWalletBalances();

            foreach (var walletBalance in walletBalances)
            {
                if (walletBalance.Currency == "BTC") continue;

                var lastTradeForPair = _databaseService.GetLastTradeForPair(walletBalance.Currency);
                if (lastTradeForPair == null) continue;
                var currentPrice = _bittrexService.GetPrice(lastTradeForPair.Terms);
                var percentage = PriceDifference(currentPrice, lastTradeForPair.Limit);

                if (percentage > 30)
                {
                    await _bus.SendAsync(new SendBagNotificationCommand(walletBalance, lastTradeForPair, currentPrice,
                        percentage));
                }
            }
        }

        private IEnumerable<Trade> FindNewTrades(IEnumerable<Trade> orderHistory)
        {
            _databaseService.AddTrades(orderHistory, out List<Trade> newTrades);
            return newTrades.OrderBy(x => x.TimeStamp);
        }

        private DateTime GetLastChecked(string exchange)
        {
            return _databaseService.GetLastChecked(exchange);
        }

        private IEnumerable<Trade> GetNewOrdersFromBittrex()
        {
            var lastChecked = GetLastChecked(Constants.Bittrex);
            var orderHistory = _bittrexService.GetOrderHistory(lastChecked);
            var newTrades = FindNewTrades(orderHistory);
            _databaseService.AddLastChecked(Constants.Bittrex, DateTime.Now);
            return newTrades;
        }

        private IEnumerable<Trade> GetNewOrdersFromPolo()
        {
            var lastChecked = GetLastChecked(Constants.Poloniex);
            var orderHistory = _poloniexService.GetOrderHistory(lastChecked);
            var newTrades = FindNewTrades(orderHistory);
            _databaseService.AddLastChecked(Constants.Poloniex, DateTime.Now);
            return newTrades;
        }

        private decimal PriceDifference(decimal currentPrice, decimal limit)
        {
            var percentage = (currentPrice - limit) / limit * 100;
            return Math.Round(percentage, 0);
        }

        private async Task SendNewTradeNotificationsOnStartup(IEnumerable<Trade> newTrades)
        {
            var i = 0;

            foreach (var newTrade in newTrades)
            {
                if (i >= 4) break;

                await _bus.SendAsync(new TradeNotificationCommand(newTrade));
                i++;
            }
        }
    }
}