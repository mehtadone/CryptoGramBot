using System.Text;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using FluentScheduler;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.EventBus.Handlers;
using CryptoGramBot.Helpers;
using Enexure.MicroBus;

namespace CryptoGramBot.Services
{
    public class StartupCheckingService
    {
        private readonly BagConfig _bagConfig;
        private readonly BittrexConfig _bittrexConfig;
        private readonly TelegramBot _bot;
        private readonly IMicroBus _bus;
        private readonly CoinigyConfig _coinigyConfig;
        private readonly DustConfig _dustConfig;
        private readonly LowBtcConfig _lowBtcConfig;
        private readonly PoloniexConfig _poloniexConfig;
        private readonly TelegramConfig _telegramConfig;

        public StartupCheckingService(
            IMicroBus bus,
            TelegramConfig telegramConfig,
            TelegramBot bot,
            CoinigyConfig coinigyConfig,
            PoloniexConfig poloniexConfig,
            BittrexConfig bittrexConfig,
            DustConfig dustConfig,
            BagConfig bagConfig,
            LowBtcConfig lowBtcConfig
            )
        {
            _bus = bus;
            _telegramConfig = telegramConfig;
            _bot = bot;
            _coinigyConfig = coinigyConfig;
            _poloniexConfig = poloniexConfig;
            _bittrexConfig = bittrexConfig;
            _dustConfig = dustConfig;
            _bagConfig = bagConfig;
            _lowBtcConfig = lowBtcConfig;
        }

        public async Task Start()
        {
            // Needs to be called here as if we use DI, the config has not been binded yet
            _bot.StartBot(_telegramConfig);

            var registry = new Registry();
            if (_bittrexConfig.Enabled || _poloniexConfig.Enabled)
            {
                SendStartupMessage().Wait();

                registry.Schedule(() => GetNewOrders().Wait())
                    .ToRunNow()
                    .AndEvery(1)
                    .Minutes();
            }

            if (_bittrexConfig.Enabled)
            {
                if (!string.IsNullOrEmpty(_bittrexConfig.DailyNotifications))
                {
                    var dailyBalance = _bittrexConfig.DailyNotifications.Split(':');
                    int.TryParse(dailyBalance[0], out int hour);
                    int.TryParse(dailyBalance[1], out int min);

                    registry.Schedule(() => DailyBalanceCheck(Constants.Bittrex).Wait()).ToRunEvery(1).Days().At(hour, min);
                }
            }

            if (_poloniexConfig.Enabled)
            {
                if (!string.IsNullOrEmpty(_poloniexConfig.DailyNotifications))
                {
                    var dailyBalance = _poloniexConfig.DailyNotifications.Split(':');
                    int.TryParse(dailyBalance[0], out int hour);
                    int.TryParse(dailyBalance[1], out int min);

                    registry.Schedule(() => DailyBalanceCheck(Constants.Poloniex).Wait()).ToRunEvery(1).Days().At(hour, min);
                }
            }

            if (_bittrexConfig.Enabled || _poloniexConfig.Enabled || _coinigyConfig.Enabled)
            {
                registry.Schedule(() => CheckBalances().Wait()).ToRunNow().AndEvery(1).Hours().At(0);
                registry.Schedule(() => CheckDepositAndWithdrawals().Wait()).ToRunEvery(2).Minutes();
            }

            if (_bagConfig.Enabled || _lowBtcConfig.Enabled || _dustConfig.Enabled)
            {
                registry.Schedule(() => CheckForBags().Wait()).ToRunEvery(8).Hours();
                registry.Schedule(() => CheckForBags().Wait()).ToRunOnceIn(5).Minutes();
            }

            if (_coinigyConfig.Enabled)
            {
                await _bus.SendAsync(new GetCoinigyAccountCommand());
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

        private async Task DailyBalanceCheck(string exchange)
        {
            var balanceCheckEvent = new BalanceCheckEvent(true, exchange);
            await _bus.PublishAsync(balanceCheckEvent);
        }

        private async Task GetNewOrders()
        {
            await _bus.PublishAsync(new NewTradesCheckEvent());
        }

        // Need to do this or we end may end up with 500 + messages on first run
        private async Task SendStartupMessage()
        {
            const string message = "<strong>Welcome to CryptoGramBot. I am currently querying for your trade history. Type /help for commands.</strong>\n";
            await _bus.SendAsync(new SendMessageCommand(new StringBuilder(message)));
        }
    }
}