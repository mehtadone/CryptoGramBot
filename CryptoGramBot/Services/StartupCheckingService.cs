using System;
using System.Threading.Tasks;
using CryptoGramBot.Configuration;
using FluentScheduler;
using CryptoGramBot.EventBus.Events;
using CryptoGramBot.EventBus.Handlers;
using CryptoGramBot.EventBus.Handlers.Binance;
using CryptoGramBot.EventBus.Handlers.Coinigy;
using CryptoGramBot.Helpers;
using Enexure.MicroBus;

namespace CryptoGramBot.Services
{
    public class StartupCheckingService
    {
        private readonly BinanceConfig _binanceConfig;
        private readonly BittrexConfig _bittrexConfig;
        private readonly TelegramBot _bot;
        private readonly IMicroBus _bus;
        private readonly CoinigyConfig _coinigyConfig;
        private readonly PoloniexConfig _poloniexConfig;
        private readonly TelegramConfig _telegramConfig;

        public StartupCheckingService(
            IMicroBus bus,
            TelegramConfig telegramConfig,
            TelegramBot bot,
            CoinigyConfig coinigyConfig,
            PoloniexConfig poloniexConfig,
            BittrexConfig bittrexConfig,
            BinanceConfig binanceConfig
            )
        {
            _bus = bus;
            _telegramConfig = telegramConfig;
            _bot = bot;
            _coinigyConfig = coinigyConfig;
            _poloniexConfig = poloniexConfig;
            _bittrexConfig = bittrexConfig;
            _binanceConfig = binanceConfig;
        }

        public void Start()
        {
            // Needs to be called here as if we use DI, the config has not been binded yet
            _bot.StartBot(_telegramConfig);

            var registry = new Registry();

            if (_bittrexConfig.Enabled || _poloniexConfig.Enabled || _binanceConfig.Enabled)
            {   
                registry.Schedule(() => GetNewOrders().Wait())
                    .NonReentrant() //first time account trades request it is longest than one minute, and will be good if we will wait end of requests and next sheduler will run after fully completed previous.
                    .ToRunEvery(1)
                    .Minutes();
            }

            if (_bittrexConfig.Enabled)
            {
                if (!string.IsNullOrEmpty(_bittrexConfig.DailyNotifications))
                {
                    var dailyBalance = _bittrexConfig.DailyNotifications.Split(':');
                    int.TryParse(dailyBalance[0], out var hour);
                    int.TryParse(dailyBalance[1], out var min);

                    registry.Schedule(() => DailyBalanceCheck(Constants.Bittrex).Wait()).ToRunEvery(1).Days().At(hour, min);
                }
            }

            if (_binanceConfig.Enabled)
            {
                if (!string.IsNullOrEmpty(_bittrexConfig.DailyNotifications))
                {
                    var dailyBalance = _bittrexConfig.DailyNotifications.Split(':');
                    int.TryParse(dailyBalance[0], out var hour);
                    int.TryParse(dailyBalance[1], out var min);

                    registry.Schedule(() => DailyBalanceCheck(Constants.Binance).Wait()).ToRunEvery(1).Days().At(hour, min);
                }
            }

            if (_poloniexConfig.Enabled)
            {
                if (!string.IsNullOrEmpty(_poloniexConfig.DailyNotifications))
                {
                    var dailyBalance = _poloniexConfig.DailyNotifications.Split(':');
                    int.TryParse(dailyBalance[0], out var hour);
                    int.TryParse(dailyBalance[1], out var min);

                    registry.Schedule(() => DailyBalanceCheck(Constants.Poloniex).Wait()).ToRunEvery(1).Days().At(hour, min);
                }
            }

            if (_bittrexConfig.Enabled || _poloniexConfig.Enabled || _binanceConfig.Enabled || _coinigyConfig.Enabled)
            {
                registry.Schedule(() => CheckBalances().Wait()).ToRunEvery(1).Hours().At(0);
                registry.Schedule(() => CheckBalances().Wait()).ToRunOnceIn(30).Seconds();
                registry.Schedule(() => CheckDepositAndWithdrawals().Wait()).ToRunEvery(2).Minutes();
            }

            registry.Schedule(() => CheckForBags().Wait()).ToRunEvery(8).Hours();

            if (_coinigyConfig.Enabled)
            {
                registry.Schedule(() => GetCoinigyAccounts().Wait()).ToRunOnceIn(3).Minutes();
            }

            registry.Schedule(() => ForceGC()).ToRunEvery(15).Minutes();

            JobManager.Initialize(registry);

            SendStartupMessage().Wait();
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

        private void ForceGC()
        {
            GC.Collect();
        }

        private async Task GetCoinigyAccounts()
        {
            await _bus.SendAsync(new GetCoinigyAccountCommand());
        }

        private async Task GetNewOrders()
        {
            await _bus.PublishAsync(new NewTradesCheckEvent());
        }

        // Need to do this or we end may end up with 500 + messages on first run
        private async Task SendStartupMessage()
        {
            var message = new StringBuffer();
            message.Append(StringContants.Welcome);
            await _bus.SendAsync(new SendMessageCommand(message));
        }
    }
}