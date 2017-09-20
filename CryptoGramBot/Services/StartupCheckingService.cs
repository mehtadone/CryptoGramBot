using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using FluentScheduler;
using CryptoGramBot.EventBus;
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
                registry.Schedule(() => CheckBalances().Wait()).ToRunNow().AndEvery(1).Hours().At(0);
            }

            if (bagManagementEnabled)
            {
                registry.Schedule(() => CheckForBags().Wait()).ToRunNow().AndEvery(6).Hours();
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