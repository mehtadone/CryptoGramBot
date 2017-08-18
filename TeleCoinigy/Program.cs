using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentScheduler;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using TeleCoinigy.Configuration;
using TeleCoinigy.Services;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TeleCoinigy.Database;
using TeleCoinigy.Models;

namespace TeleCoinigy
{
    internal class Program
    {
        private static MainService _mainService;

        private static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            var coinigyConfig = new CoinigyConfig();
            configuration.GetSection("Coinigy").Bind(coinigyConfig);

            var telegramConfig = new TelegramConfig();
            configuration.GetSection("Telegram").Bind(telegramConfig);

            var frequencyConfig = new FrequencyConfig();
            configuration.GetSection("Frequency").Bind(frequencyConfig);

            _mainService = new MainService(coinigyConfig, telegramConfig);

            var registry = new Registry();

            registry.Schedule(() => _mainService.SendAccountInfo().Wait()).ToRunOnceIn(5).Seconds();
            registry.Schedule(() => _mainService.SendTotalBalanceUpdate().Wait()).ToRunOnceIn(10).Seconds();
            registry.Schedule(() => _mainService.SendTotalBalanceUpdate().Wait()).ToRunEvery(frequencyConfig.HoursForTotalBalance).Hours();
            registry.Schedule(() => _mainService.SendSpecificAccountDetails().Wait()).ToRunEvery(frequencyConfig.HoursForSpecificBalance).Hours();

            JobManager.Initialize(registry);

            while (true)
            {
                Console.ReadKey();
            };//This wont stop app
        }
    }
}