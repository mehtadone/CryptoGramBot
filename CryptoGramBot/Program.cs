using System;
using System.Collections.Generic;
using System.IO;
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
using Enexure.MicroBus.Autofac;

namespace CryptoGramBot
{
    internal class Program
    {
        private static void ConfigureConfig(AutofacServiceProvider serviceCollection, IConfigurationRoot configuration, ILogger<Program> log)
        {
            var coinigyConfig = serviceCollection.GetService<CoinigyConfig>();
            configuration.GetSection("Coinigy").Bind(coinigyConfig);
            log.LogInformation("Created Coinigy Config");

            var telegramConfig = serviceCollection.GetService<TelegramConfig>();
            configuration.GetSection("Telegram").Bind(telegramConfig);
            log.LogInformation("Created Telegram Config");

            var bittrexConfig = serviceCollection.GetService<BittrexConfig>();
            configuration.GetSection("Bittrex").Bind(bittrexConfig);
            log.LogInformation("Created bittrex Config");
        }

        private static void ConfigureLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.RollingFile(Directory.GetCurrentDirectory() + "\\logs\\CryptoGramBot.log")
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        private static ContainerBuilder ConfigureServices()
        {
            var serviceCollection = new ServiceCollection()
                .AddLogging()
                .AddScoped<CoinigyConfig>()
                .AddScoped<TelegramConfig>()
                .AddScoped<BittrexConfig>()
                .AddScoped<CoinigyApiService>()
                .AddScoped<BittrexService>()
                .AddScoped<DatabaseService>()
                .AddSingleton<TelegramService>()
                .AddSingleton<StartupService>()
                .AddSingleton<BalanceService>()
                .AddScoped<IExchange, Exchange>();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(serviceCollection);

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
            var container = containerBuilder.Build();

            var busBuilder = new BusBuilder();
            busBuilder.ConfigureCore();

            containerBuilder.RegisterMicroBus(busBuilder);

            var serviceCollection = new AutofacServiceProvider(container);

            serviceCollection.GetService<ILoggerFactory>()
                .AddSerilog();

            var logger = serviceCollection.GetService<ILoggerFactory>();
            var log = logger.CreateLogger<Program>();

            ConfigureConfig(serviceCollection, configuration, log);

            var startupService = serviceCollection.GetService<StartupService>();
            startupService.Start();

            while (true)
            {
                Console.ReadKey();
            };//This wont stop app
        }
    }
}