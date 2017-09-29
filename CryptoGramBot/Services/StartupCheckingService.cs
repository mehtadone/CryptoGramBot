using System.IO;
using System.Linq;
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
            var allSettings = _context.Settings;

            if (!File.Exists(Directory.GetCurrentDirectory() + "/database/cryptogrambot.db")) return;

            var hasMigratedBefore = allSettings.SingleOrDefault(x => x.Name == "SQLite.Migration.Complete");

            if (hasMigratedBefore != null && hasMigratedBefore.Value != "true")
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

                var setting = new Setting
                {
                    Name = "SQLite.Migration.Complete",
                    Value = "true"
                };

                _context.Settings.Add(setting);

                await _context.SaveChangesAsync();
            }
        }

        public void Start(bool coinigyEnabled, bool bittrexEnabled, bool poloEnabled, bool bagManagementEnabled)
        {
            // Needs to be called here as if we use DI, the config has not been binded yet
            _bot.StartBot(_telegramConfig);

            var registry = new Registry();
            if (bittrexEnabled || poloEnabled)
            {
                SendStatupMessage().Wait();

                registry.Schedule(() => GetNewOrders().Wait())
                    .ToRunNow()
                    .AndEvery(5)
                    .Minutes();
            }

            if (bittrexEnabled || poloEnabled || coinigyEnabled)
            {
                registry.Schedule(() => CheckBalances().Wait()).ToRunNow().AndEvery(1).Hours().At(0);
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
            await _bus.PublishAsync(new BalanceCheckEvent(false));
        }

        private async Task CheckForBags()
        {
            await _bus.PublishAsync(new BagAndDustEvent());
        }

        private async Task GetNewOrders()
        {
            await _bus.PublishAsync(new NewTradesCheckEvent());
        }

        // Need to do this or we end may end up with 500 + messages on first run
        private async Task SendStatupMessage()
        {
            const string message = "<strong>Welcome to CryptoGramBot. I am currently querying for your trade history. Type /help for commands.</strong>\n";
            await _bus.SendAsync(new SendMessageCommand(message));
        }
    }
}