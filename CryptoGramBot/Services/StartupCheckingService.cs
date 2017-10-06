using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using FluentScheduler;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.EventBus.Handlers;
using Enexure.MicroBus;

namespace CryptoGramBot.Services
{
    public class StartupCheckingService
    {
        private readonly TelegramBot _bot;
        private readonly IMicroBus _bus;
        private readonly TelegramConfig _telegramConfig;

        public StartupCheckingService(
            IMicroBus bus,
            TelegramConfig telegramConfig,
            TelegramBot bot
            )
        {
            _bus = bus;
            _telegramConfig = telegramConfig;
            _bot = bot;
        }

        public void Start(bool coinigyEnabled, bool bittrexEnabled, bool poloEnabled, bool bagManagementEnabled, bool lowBtcNotification, bool dustNotifications)
        {
            // Needs to be called here as if we use DI, the config has not been binded yet
            _bot.StartBot(_telegramConfig);

            var registry = new Registry();
            if (bittrexEnabled || poloEnabled)
            {
                SendStartupMessage().Wait();

                registry.Schedule(() => GetNewOrders().Wait())
                    .ToRunNow()
                    .AndEvery(5)
                    .Minutes();
            }

            if (bittrexEnabled || poloEnabled || coinigyEnabled)
            {
                registry.Schedule(() => CheckBalances().Wait()).ToRunNow().AndEvery(1).Hours().At(0);
                registry.Schedule(() => CheckDepositAndWithdrawals().Wait()).ToRunEvery(2).Minutes();
            }

            if (bagManagementEnabled || lowBtcNotification || dustNotifications)
            {
                registry.Schedule(() => CheckForBags().Wait()).ToRunEvery(6).Hours();
                registry.Schedule(() => CheckForBags().Wait()).ToRunOnceIn(5).Minutes();
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

        private async Task CheckDepositAndWithdrawals()
        {
            await _bus.PublishAsync(new DepositAndWithdrawalEvent());
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
        private async Task SendStartupMessage()
        {
            const string message = "<strong>Welcome to CryptoGramBot. I am currently querying for your trade history. Type /help for commands.</strong>\n";
            await _bus.SendAsync(new SendMessageCommand(message));
        }
    }
}