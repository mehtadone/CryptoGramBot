using System;
using System.IO;
using System.Linq;
using Autofac;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using CryptoGramBot.Configuration;
using CryptoGramBot.Services;
using CryptoGramBot.Extensions;
using Enexure.MicroBus;
using Autofac.Extensions.DependencyInjection;
using CryptoGramBot.Data;
using CryptoGramBot.Services.Exchanges;
using CryptoGramBot.Services.Telegram;
using Enexure.MicroBus.Autofac;
using Microsoft.EntityFrameworkCore;
using IConfigurationProvider = Microsoft.Extensions.Configuration.IConfigurationProvider;
using System.Reflection;

namespace CryptoGramBot
{
    internal class Program
    {
        private static void CheckWhatIsEnabled(
            IConfigurationProvider provider,
            out bool coinigyEnabled,
            out bool bittrexEnabled,
            out bool poloniexEnabled,
            out bool bagEnabled,
            out bool dustNotification,
            out bool lowBtcNotification)
        {
            provider.TryGet("Coinigy:Enabled", out string coinigyEnabledString);
            provider.TryGet("Bittrex:Enabled", out string bittrexEnabledString);
            provider.TryGet("Poloniex:Enabled", out string poloniexEnabledString);
            provider.TryGet("BagManagement:Enabled", out string bagManagementEnabledString);
            provider.TryGet("DustNotification:Enabled", out string dustNotifcationEnabledString);
            provider.TryGet("LowBtcNotification:Enabled", out string lowBtcNotificationString);

            coinigyEnabled = bool.Parse(coinigyEnabledString);
            bittrexEnabled = bool.Parse(bittrexEnabledString);
            poloniexEnabled = bool.Parse(poloniexEnabledString);

            if (bittrexEnabled || poloniexEnabled)
            {
                bagEnabled = bool.Parse(bagManagementEnabledString);
                dustNotification = bool.Parse(dustNotifcationEnabledString);
                lowBtcNotification = bool.Parse(lowBtcNotificationString);
            }
            else
            {
                bagEnabled = false;
                dustNotification = false;
                lowBtcNotification = false;
            }
        }

        private static void ConfigureConfig(IContainer container, IConfigurationRoot configuration, ILogger<Program> log)
        {
            try
            {
                var config = container.Resolve<CoinigyConfig>();
                configuration.GetSection("Coinigy").Bind(config);
                log.LogInformation("Created Coinigy Config");
            }
            catch (Exception)
            {
                log.LogError("Error in reading Coinigy Config");
                throw;
            }

            try
            {
                var config = container.Resolve<GeneralConfig>();
                configuration.GetSection("General").Bind(config);
                log.LogInformation("Created General Config");
            }
            catch (Exception)
            {
                log.LogError("Error in reading General Config");
                throw;
            }

            try
            {
                var config = container.Resolve<TelegramConfig>();
                configuration.GetSection("Telegram").Bind(config);
                log.LogInformation("Created Telegram Config");
            }
            catch (Exception)
            {
                log.LogError("Error in reading telegram config");
                throw;
            }

            try
            {
                var config = container.Resolve<BittrexConfig>();
                configuration.GetSection("Bittrex").Bind(config);
                log.LogInformation("Created bittrex Config");
            }
            catch (Exception)
            {
                log.LogError("Error in reading bittrex config");
                throw;
            }

            try
            {
                var config = container.Resolve<PoloniexConfig>();
                configuration.GetSection("Poloniex").Bind(config);
                log.LogInformation("Created Poloniex Config");
            }
            catch (Exception)
            {
                log.LogError("Error in reading telegram config");
                throw;
            }

            try
            {
                var config = container.Resolve<BagConfig>();
                configuration.GetSection("BagManagement").Bind(config);
                log.LogInformation("Created Bag Management Config");
            }
            catch (Exception)
            {
                log.LogError("Error in reading bag management config");
                throw;
            }

            try
            {
                var config = container.Resolve<DustConfig>();
                configuration.GetSection("DustNotification").Bind(config);
                log.LogInformation("Created dust notification Config");
            }
            catch (Exception)
            {
                log.LogError("Error in reading dust notification config");
                throw;
            }

            try
            {
                var config = container.Resolve<LowBtcConfig>();
                configuration.GetSection("LowBtcNotification").Bind(config);
                log.LogInformation("Created low btc Config");
            }
            catch (Exception)
            {
                log.LogError("Error in reading low btc config");
                throw;
            }
        }

        private static void ConfigureLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.RollingFile(Directory.GetCurrentDirectory() + "/logs/CryptoGramBot.log")
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .CreateLogger();
        }

        private static ContainerBuilder ConfigureServices()
        {
            var serviceCollection = new ServiceCollection()
                .AddLogging();

            var databaseLocation = Directory.GetCurrentDirectory() + "/database/cryptogrambot.sqlite";

            serviceCollection.AddDbContext<CryptoGramBotDbContext>(options =>
                options.UseSqlite("Data Source=" + databaseLocation + ";cache=shared")
            );

            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(serviceCollection);

            containerBuilder.RegisterType<CoinigyConfig>().SingleInstance();
            containerBuilder.RegisterType<TelegramConfig>().SingleInstance();
            containerBuilder.RegisterType<BittrexConfig>().SingleInstance();
            containerBuilder.RegisterType<PoloniexConfig>().SingleInstance();
            containerBuilder.RegisterType<BagConfig>().SingleInstance();
            containerBuilder.RegisterType<DustConfig>().SingleInstance();
            containerBuilder.RegisterType<GeneralConfig>().SingleInstance();
            containerBuilder.RegisterType<LowBtcConfig>().SingleInstance();
            containerBuilder.RegisterType<CoinigyApiService>();
            containerBuilder.RegisterType<BittrexService>();
            containerBuilder.RegisterType<PoloniexService>();
            containerBuilder.RegisterType<DatabaseService>();
            containerBuilder.RegisterType<TelegramMessageRecieveService>().SingleInstance();
            containerBuilder.RegisterType<TelegramMessageSendingService>().SingleInstance();
            containerBuilder.RegisterType<StartupCheckingService>().SingleInstance();
            containerBuilder.RegisterType<CoinigyBalanceService>();
            containerBuilder.RegisterType<TelegramBot>().SingleInstance();
            containerBuilder.RegisterType<PriceService>().SingleInstance();
            containerBuilder.RegisterType<ProfitAndLossService>();
            containerBuilder.RegisterType<TradeExportService>();
            containerBuilder.RegisterType<TelegramBittrexFileUploadService>();
            containerBuilder.RegisterType<TelegramPairProfitService>();

            var assembly = Directory.GetFiles(Assembly.GetExecutingAssembly().Location, "*.dll")
                                    .SingleOrDefault(a => a.Contains("CryptoGramBag"));
            if (assembly != null)
            {
                var asm = Assembly.LoadFrom(assembly);
                containerBuilder.RegisterAssemblyTypes(asm)
                                .Where(t => t == typeof(IPlugin))
                                .AsImplementedInterfaces();
            }

            return containerBuilder;
        }

        private static void Main(string[] args)
        {
            ConfigureLogger();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true);

            IConfigurationRoot configuration = builder.Build();

            Mapper.Initialize(config => config.MapEntities());

            var containerBuilder = ConfigureServices();

            // We only have one settings provider so this works for the moment
            var provider = configuration.Providers.First();

            CheckWhatIsEnabled(provider,
                out bool coinigyEnabled,
                out bool bittrexEnabled,
                out bool poloniexEnabled,
                out bool bagEnabled,
                out bool dustEnabled,
                out bool lowBtcEnabled);

            var busBuilder = new BusBuilder();

            busBuilder.ConfigureCore(coinigyEnabled, bittrexEnabled, poloniexEnabled, bagEnabled, dustEnabled);

            containerBuilder.RegisterMicroBus(busBuilder);
            var container = containerBuilder.Build();

            var loggerFactory = container.Resolve<ILoggerFactory>().AddSerilog();
            var log = loggerFactory.CreateLogger<Program>();

            log.LogInformation($"Services\nCoinigy: {coinigyEnabled}\nBittrex: {bittrexEnabled}\nPoloniex: {poloniexEnabled}\nBag Management: {bagEnabled}\nDust Notifications: {dustEnabled}");
            ConfigureConfig(container, configuration, log);

            var startupService = container.Resolve<StartupCheckingService>();
            var context = container.Resolve<CryptoGramBotDbContext>();

            DbInitializer.Initialize(context).Wait();

            startupService.Start();

            while (true)
            {
                Console.ReadKey();
            }; //This wont stop app
        }
    }
}