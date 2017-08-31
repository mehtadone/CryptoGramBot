using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentScheduler;
using Newtonsoft.Json;
using TeleCoinigy.Database;
using TeleCoinigy.Models;

namespace TeleCoinigy.Services
{
    public class StartupService
    {
        private readonly BalanceService _balanceService;
        private readonly BittrexService _bittrexService;
        private readonly DatabaseService _databaseService;
        private readonly TelegramService _telegramService;

        public StartupService(
            BittrexService bittrexService,
            DatabaseService databaseService,
            BalanceService balanceService,
            TelegramService telegramService)
        {
            _bittrexService = bittrexService;
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
            var newTrades = GetNewOrdersFromBittrex();

            foreach (var newTrade in newTrades)
            {
                await _telegramService.SendTradeNotification(newTrade);
            }
        }

        public async Task GetNewOrdersOnStartup()
        {
            var newTrades = GetNewOrdersFromBittrex();

            var i = 0;

            var message = "<strong>Checking new orders on startup. Will only send top 5</strong>\n";
            await _telegramService.SendMessage(message);

            foreach (var newTrade in newTrades)
            {
                if (i >= 4) break;

                await _telegramService.SendTradeNotification(newTrade);
                i++;
            }
        }

        public void Start()
        {
            var registry = new Registry();
            registry.Schedule(() => GetNewOrdersOnStartup().Wait()).ToRunNow();
            registry.Schedule(() => GetNewOrders().Wait()).ToRunEvery(5).Minutes();
            registry.Schedule(() => CheckCoinigyBalances().Wait()).ToRunNow().AndEvery(1).Hours().At(0);

            JobManager.Initialize(registry);

            _telegramService.StartBot();
        }

        private IEnumerable<Trade> GetNewOrdersFromBittrex()
        {
            var orderHistory = _bittrexService.GetOrderHistory();
            _databaseService.AddTrades(orderHistory, out List<Trade> newTrades);
            return newTrades;
        }
    }
}