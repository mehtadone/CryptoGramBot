using System;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using CryptoGramBot.Configuration;
using CryptoGramBot.Data;
using CryptoGramBot.Extensions;
using CryptoGramBot.Services;
using CryptoGramBot.Services.Exchanges;
using CryptoGramBot.Services.Pricing;
using CryptoGramBot.Services.Telegram;
using Enexure.MicroBus;
using Enexure.MicroBus.Autofac;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CryptoGramBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            TelemetryConfiguration.Active.DisableTelemetry = true;

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterInstance(Configuration).As<IConfiguration>().SingleInstance();

            var serviceCollection = new ServiceCollection().AddLogging(loggingBuilder =>
                loggingBuilder.AddSerilog(dispose: true));

            var databaseLocation = Directory.GetCurrentDirectory() + "/database/cryptogrambot.sqlite";

            serviceCollection.AddDbContext<CryptoGramBotDbContext>(options =>
                options.UseSqlite("Data Source=" + databaseLocation + ";cache=shared")
            );

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

            Mapper.Initialize(config => config.MapEntities());

            CheckWhatIsEnabled(out bool coinigyEnabled,
                out bool bittrexEnabled,
                out bool poloniexEnabled,
                out bool bagEnabled,
                out bool dustEnabled,
                out bool lowBtcEnabled);

            var busBuilder = new BusBuilder();

            busBuilder.ConfigureCore(coinigyEnabled, bittrexEnabled, poloniexEnabled, bagEnabled, dustEnabled);

            containerBuilder.RegisterMicroBus(busBuilder);
            var container = containerBuilder.Build();

            var loggerFactory = container.Resolve<ILoggerFactory>();
            var log = loggerFactory.CreateLogger<Program>();

            log.LogInformation($"Services\nCoinigy: {coinigyEnabled}\nBittrex: {bittrexEnabled}\nPoloniex: {poloniexEnabled}\nBag Management: {bagEnabled}\nDust Notifications: {dustEnabled}\nLow BTC Notifications: {lowBtcEnabled}");
            ConfigureConfig(container, Configuration, log);

            var startupService = container.Resolve<StartupCheckingService>();
            var context = container.Resolve<CryptoGramBotDbContext>();

            DbInitializer.Initialize(context).Wait();

            startupService.Start().Wait();
        }

        private static void ConfigureConfig(IContainer container, IConfiguration configuration, ILogger<Program> log)
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

        private void CheckWhatIsEnabled(
                    out bool coinigyEnabled,
            out bool bittrexEnabled,
            out bool poloniexEnabled,
            out bool bagEnabled,
            out bool dustNotification,
            out bool lowBtcNotification)
        {
            string coinigyEnabledString = Configuration.GetSection("Coinigy").GetValue("Enabled", "false");
            string bittrexEnabledString = Configuration.GetSection("Bittrex").GetValue("Enabled", "false");
            string poloniexEnabledString = Configuration.GetSection("Poloniex").GetValue("Enabled", "false");
            string bagManagementEnabledString = Configuration.GetSection("BagManagement").GetValue("Enabled", "false");
            string dustNotifcationEnabledString = Configuration.GetSection("DustNotification").GetValue("Enabled", "false");
            string lowBtcNotificationString = Configuration.GetSection("LowBtcNotification").GetValue("Enabled", "false");

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
    }
}