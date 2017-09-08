using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Autofac;
using AutoMapper;
using Bittrex;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using CryptoGramBot.Configuration;
using CryptoGramBot.Services;
using CryptoGramBot.Database;
using CryptoGramBot.Extensions;
using Enexure.MicroBus;
using Autofac.Extensions.DependencyInjection;
using CryptoGramBot.EventBus;
using Enexure.MicroBus.Autofac;

namespace CryptoGramBot
{
    internal class Program
    {
        private static void ConfigureConfig(IContainer container, IConfigurationRoot configuration, ILogger<Program> log)
        {
            var coinigyConfig = container.Resolve<CoinigyConfig>();
            configuration.GetSection("Coinigy").Bind(coinigyConfig);
            log.LogInformation("Created Coinigy Config");

            var telegramConfig = container.Resolve<TelegramConfig>();
            configuration.GetSection("Telegram").Bind(telegramConfig);
            log.LogInformation("Created Telegram Config");

            var bittrexConfig = container.Resolve<BittrexConfig>();
            configuration.GetSection("Bittrex").Bind(bittrexConfig);
            log.LogInformation("Created bittrex Config");

            var poloniexConfig = container.Resolve<PoloniexConfig>();
            configuration.GetSection("Poloniex").Bind(poloniexConfig);
            log.LogInformation("Created Poloniex Config");

            var bagConfig = container.Resolve<BagConfig>();
            configuration.GetSection("BagManagement").Bind(bagConfig);
            log.LogInformation("Created Bag Management Config");
        }

        private static void ConfigureLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.RollingFile(Directory.GetCurrentDirectory() + "/logs/CryptoGramBot.log")
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        private static ContainerBuilder ConfigureServices()
        {
            var serviceCollection = new ServiceCollection()
                .AddLogging();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(serviceCollection);

            containerBuilder.RegisterType<CoinigyConfig>().SingleInstance();
            containerBuilder.RegisterType<TelegramConfig>().SingleInstance();
            containerBuilder.RegisterType<BittrexConfig>().SingleInstance();
            containerBuilder.RegisterType<PoloniexConfig>().SingleInstance();
            containerBuilder.RegisterType<BagConfig>().SingleInstance();
            containerBuilder.RegisterType<CoinigyApiService>();
            containerBuilder.RegisterType<BittrexService>();
            containerBuilder.RegisterType<PoloniexService>();
            containerBuilder.RegisterType<DatabaseService>().SingleInstance();
            containerBuilder.RegisterType<TelegramMessageRecieveService>().SingleInstance();
            containerBuilder.RegisterType<CheckingService>().SingleInstance();
            containerBuilder.RegisterType<BalanceService>();
            containerBuilder.RegisterType<TelegramBot>().SingleInstance();
            containerBuilder.RegisterType<Exchange>().As<IExchange>();

            return containerBuilder;
        }

        private static void Main(string[] args)
        {
            ConfigureLogger();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            Mapper.Initialize(config => config.MapEntities());

            var containerBuilder = ConfigureServices();

            var busBuilder = new BusBuilder();
            busBuilder.ConfigureCore();

            containerBuilder.RegisterMicroBus(busBuilder);

            var container = containerBuilder.Build();

            var loggerFactory = container.Resolve<ILoggerFactory>().AddSerilog();
            var log = loggerFactory.CreateLogger<Program>();

            ConfigureConfig(container, configuration, log);

            var startupService = container.Resolve<CheckingService>();
            startupService.Start();

            while (true)
            {
                Console.ReadKey();
            };//This wont stop app
        }
    }
}