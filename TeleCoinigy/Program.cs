using System;
using System.IO;
using AutoMapper;
using Bittrex;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using TeleCoinigy.Configuration;
using TeleCoinigy.Services;
using TeleCoinigy.Database;
using TeleCoinigy.Extensions;

namespace TeleCoinigy
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.RollingFile(".\\logs\\TeleCoinigy.log")
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .CreateLogger();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            Mapper.Initialize(config => config.MapEntities());

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
                .AddScoped<IExchange, Exchange>()
                .BuildServiceProvider();

            serviceCollection.GetService<ILoggerFactory>()
                .AddSerilog();

            var logger = serviceCollection.GetService<ILoggerFactory>();
            var log = logger.CreateLogger<Program>();

            var coinigyConfig = serviceCollection.GetService<CoinigyConfig>();
            configuration.GetSection("Coinigy").Bind(coinigyConfig);
            log.LogInformation("Created Coinigy Config");

            var telegramConfig = serviceCollection.GetService<TelegramConfig>();
            configuration.GetSection("Telegram").Bind(telegramConfig);
            log.LogInformation("Created Telegram Config");

            var bittrexConfig = serviceCollection.GetService<BittrexConfig>();
            configuration.GetSection("Bittrex").Bind(bittrexConfig);
            log.LogInformation("Created bittrex Config");

            var startupService = serviceCollection.GetService<StartupService>();
            startupService.Start();

            while (true)
            {
                Console.ReadKey();
            };//This wont stop app
        }
    }
}