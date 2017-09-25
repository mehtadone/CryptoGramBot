using System.IO;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using CryptoGramBot.Data;
using FluentScheduler;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.EventBus.Handlers;
using CryptoGramBot.Models;
using Enexure.MicroBus;

namespace CryptoGramBot.Services
{
    public class StartupCheckingService
    {
        private readonly TelegramBot _bot;
        private readonly IMicroBus _bus;
        private readonly GeneralConfig _config;
        private readonly CryptoGramBotDbContext _context;
        private readonly LiteDbDatabaseService _liteDbDatabaseService;
        private readonly TelegramConfig _telegramConfig;

        public StartupCheckingService(
            IMicroBus bus,
            TelegramConfig telegramConfig,
            TelegramBot bot,
            GeneralConfig config,
            LiteDbDatabaseService liteDbDatabaseService,
            CryptoGramBotDbContext context
            )
        {
            _bus = bus;
            _telegramConfig = telegramConfig;
            _bot = bot;
            _config = config;
            _liteDbDatabaseService = liteDbDatabaseService;
            _context = context;
        }

        public async Task MigrateToSqlLite()
        {
            var balanceHistories = _liteDbDatabaseService.GetAllBalances();

            foreach (var balanceHistory in balanceHistories)
            {
                _context.Set<BalanceHistory>().Add(balanceHistory);
            }

            var allTrades = _liteDbDatabaseService.GetAllTrades();
            foreach (var allTrade in allTrades)
            {
                _context.Set<Trade>().Add(allTrade);
            }

            var allLastChecked = _liteDbDatabaseService.GetAllLastChecked();
            foreach (var lastChecked in allLastChecked)
            {
                _context.Set<LastChecked>().Add(lastChecked);
            }

            var allProfitAndLoss = _liteDbDatabaseService.GetAllProfitAndLoss();
            foreach (var pnl in allProfitAndLoss)
            {
                _context.Set<ProfitAndLoss>().Add(pnl);
            }

            _liteDbDatabaseService.Close();

            File.Delete(_config.DatabaseLocation);

            await _context.SaveChangesAsync();
        }

        public void Start(bool coinigyEnabled, bool bittrexEnabled, bool poloEnabled, bool bagManagementEnabled, bool bittrexTradeNotifcations, bool poloniexTradeNotifcation)
        {
            // Needs to be called here as if we use DI, the config has not been binded yet
            _bot.StartBot(_telegramConfig);

            var registry = new Registry();
            if (bittrexEnabled || poloEnabled)
            {
                registry
                    .Schedule(() => GetNewOrdersOnStartup(bittrexTradeNotifcations, poloniexTradeNotifcation).Wait())
                    .ToRunNow();
                registry.Schedule(() => GetNewOrders(bittrexTradeNotifcations, poloniexTradeNotifcation).Wait())
                    .ToRunEvery(5)
                    .Minutes();
            }

            if (bittrexEnabled || poloEnabled || coinigyEnabled)
            {
                registry.Schedule(() => CheckBalances().Wait()).ToRunEvery(1).Hours().At(0);
            }

            if (bagManagementEnabled)
            {
                registry.Schedule(() => CheckForBags().Wait()).ToRunEvery(6).Hours();
            }

            if (coinigyEnabled)
            {
                _bus.SendAsync(new GetCoinigyAccountCommand());
            }

            JobManager.Initialize(registry);
        }

        private async Task CheckBalances()
        {
            await _bus.PublishAsync(new BalanceCheckEvent());
        }

        private async Task CheckForBags()
        {
            await _bus.PublishAsync(new BagAndDustEvent());
        }

        private async Task GetNewOrders(bool bittrexTradeNotifcations, bool poloniexTradeNotifcation)
        {
            await _bus.PublishAsync(new NewTradesCheckEvent(false, bittrexTradeNotifcations, poloniexTradeNotifcation));
        }

        // Need to do this or we end may end up with 500 + messages on first run
        private async Task GetNewOrdersOnStartup(bool bittrexTradeNotifcations, bool poloniexTradeNotifcation)
        {
            if (bittrexTradeNotifcations && poloniexTradeNotifcation)
            {
                const string message = "<strong>Checking new orders on startup. Will only send top 5</strong>\n";
                await _bus.SendAsync(new SendMessageCommand(message));
            }

            await _bus.PublishAsync(new NewTradesCheckEvent(true, bittrexTradeNotifcations, poloniexTradeNotifcation));
        }
    }
}