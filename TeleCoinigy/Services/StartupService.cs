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
            var orderHistory = _bittrexService.GetOrderHistory();
            _databaseService.AddTrades(orderHistory, out List<Trade> newTrades);

            foreach (var newTrade in newTrades)
            {
                if (newTrade.TimeStamp > DateTime.Now.Subtract(TimeSpan.FromHours(1)))
                {
                    await _telegramService.SendTradeNotification(newTrade);
                }
            }
        }

        public void Start()
        {
            var registry = new Registry();
            registry.Schedule(() => GetNewOrders().Wait()).ToRunNow().AndEvery(5).Minutes();
            registry.Schedule(() => CheckCoinigyBalances().Wait()).ToRunNow().AndEvery(1).Hours().At(0);

            JobManager.Initialize(registry);

            _telegramService.StartBot();
        }
    }
}