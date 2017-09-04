using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentScheduler;
using CryptoGramBot.Database;
using CryptoGramBot.Helpers;
using CryptoGramBot.Models;
using Enexure.MicroBus;

namespace CryptoGramBot.Services
{
    public class StartupService
    {
        private readonly BalanceService _balanceService;
        private readonly BittrexService _bittrexService;
        private readonly IMicroBus _bus;
        private readonly DatabaseService _databaseService;
        private readonly PoloniexService _poloniexService;
        private readonly TelegramService _telegramService;

        public StartupService(
            IMicroBus bus,
            BittrexService bittrexService,
            PoloniexService poloniexService,
            DatabaseService databaseService,
            BalanceService balanceService,
            TelegramService telegramService
            )
        {
            _bus = bus;
            _bittrexService = bittrexService;
            _poloniexService = poloniexService;
            _databaseService = databaseService;
            _balanceService = balanceService;
            _telegramService = telegramService;
        }

        public async Task CheckCoinigyBalances()
        {
            await _balanceService.GetAllBalances();
        }

        public async Task GetNewOrders()
        {
            var newOrdersFromBittrex = GetNewOrdersFromBittrex();

            foreach (var newTrade in newOrdersFromBittrex)
            {
                await _telegramService.SendTradeNotification(newTrade);
            }

            var newOrdersFromPolo = GetNewOrdersFromPolo();

            foreach (var newTrade in newOrdersFromPolo)
            {
                await _telegramService.SendTradeNotification(newTrade);
            }
        }

        public async Task GetNewOrdersOnStartup()
        {
            var message = "<strong>Checking new orders on startup. Will only send top 5</strong>\n";
            await _telegramService.SendMessage(message);

            var newTradesBittrex = GetNewOrdersFromBittrex();
            await SendNewTradeNotificationsOnStartup(newTradesBittrex);

            var newTradesPolo = GetNewOrdersFromPolo();
            await SendNewTradeNotificationsOnStartup(newTradesPolo);
        }

        public void Start()
        {
            _telegramService.StartBot();

            var registry = new Registry();
            registry.Schedule(() => GetNewOrdersOnStartup().Wait()).ToRunNow();
            registry.Schedule(() => GetNewOrders().Wait()).ToRunEvery(5).Minutes();
            registry.Schedule(() => CheckCoinigyBalances().Wait()).ToRunNow().AndEvery(1).Hours().At(0);

            JobManager.Initialize(registry);
        }

        private IEnumerable<Trade> FindNewTrades(IEnumerable<Trade> orderHistory)
        {
            _databaseService.AddTrades(orderHistory, out List<Trade> newTrades);
            return newTrades.OrderByDescending(x => x.TimeStamp);
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
            _databaseService.AddLastChecked(Constants.Poloniex, DateTime.Now);
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

        private async Task SendNewTradeNotificationsOnStartup(IEnumerable<Trade> newTrades)
        {
            var i = 0;

            foreach (var newTrade in newTrades)
            {
                if (i >= 4) break;

                await _telegramService.SendTradeNotification(newTrade);
                i++;
            }
        }
    }
}