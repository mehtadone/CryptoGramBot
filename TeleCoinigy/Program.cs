using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentScheduler;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
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
        private static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            var log = new LoggerConfiguration()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.RollingFile("logs\\TeleCoinigy-{date}.log")
                .CreateLogger();

            var coinigyConfig = new CoinigyConfig();
            configuration.GetSection("Coinigy").Bind(coinigyConfig);
            log.Information("Created Coinigy Config");

            var telegramConfig = new TelegramConfig();
            configuration.GetSection("Telegram").Bind(telegramConfig);
            log.Information("Created Telegram Config");

            var coinigyService = new CoinigyApiService(coinigyConfig, log);
            var databaseService = new DatabaseService(log);
            var telegramService = new TelegramService(telegramConfig, coinigyService, databaseService, log);

            while (true)
            {
                Console.ReadKey();
            };//This wont stop app
        }
    }
}