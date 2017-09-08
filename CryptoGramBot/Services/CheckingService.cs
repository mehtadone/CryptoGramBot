using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using FluentScheduler;
using CryptoGramBot.EventBus;
using CryptoGramBot.EventBus.Events;
using Enexure.MicroBus;

namespace CryptoGramBot.Services
{
    public class CheckingService
    {
        private readonly BalanceService _balanceService;
        private readonly TelegramBot _bot;
        private readonly IMicroBus _bus;
        private readonly TelegramConfig _telegramConfig;
        private readonly TelegramMessageRecieveService _telegramMessageRecieveService;

        public CheckingService(
            IMicroBus bus,
            TelegramConfig telegramConfig,
            TelegramMessageRecieveService telegramMessageRecieveService,
            BalanceService balanceService,
            TelegramBot bot
            )
        {
            _bus = bus;
            _telegramConfig = telegramConfig;
            _telegramMessageRecieveService = telegramMessageRecieveService;
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
            await _bus.PublishAsync(new NewTradesCheckEvent(false));
        }

        public async Task GetNewOrdersOnStartup()
        {
            const string message = "<strong>Checking new orders on startup. Will only send top 5</strong>\n";
            await _bus.SendAsync(new SendMessageCommand(message));
            await _bus.PublishAsync(new NewTradesCheckEvent(true));
        }

        public void Start()
        {
            // Start the bot so we can start sending messages.
            _bot.StartBot(_telegramConfig);

            // Start the bot so we can start receiving messages
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
            await _bus.SendAsync(new BagManagementCommand());
        }
    }
}