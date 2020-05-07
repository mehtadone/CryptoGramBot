using Autofac;
using Autofac.Extensions.DependencyInjection;
using Binance;
using Bittrex.Net;
using CryptoGramBot.Configuration;
using CryptoGramBot.Data;
using CryptoGramBot.Extensions;
using CryptoGramBot.Helpers;
using CryptoGramBot.Services;
using CryptoGramBot.Services.Data;
using CryptoGramBot.Services.Exchanges;
using CryptoGramBot.Services.Exchanges.WebSockets.Binance;
using CryptoGramBot.Services.Pricing;
using CryptoGramBot.Services.Telegram;
using Enexure.MicroBus;
using Enexure.MicroBus.Autofac;
using Jojatekok.PoloniexAPI;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bittrex.Net.Objects;
using CryptoExchange.Net.RateLimiter;

namespace CryptoGramBot
{
    public class Startup
    {
        #region Fields

        private readonly int KEEP_ALIVE_PERIOD_IN_MILLISECONDS = 900000;

        #endregion

        #region Properties

        public IConfiguration Configuration { get; }

        public IContainer Container { get; set; }

        #endregion

        #region Constructor

        public Startup(IConfiguration configuration)
        {
            TelemetryConfiguration.Active.DisableTelemetry = true;
            Configuration = configuration;
        }

        #endregion


        #region Startup

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {
            applicationLifetime.ApplicationStarted.Register(async () => await OnStarting());

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

            serviceCollection.AddDbContextPool<CryptoGramBotDbContext>(options =>
                options.UseSqlite(StringContants.DatabaseLocation)
            );

            serviceCollection.AddBinance();

            serviceCollection.BuildServiceProvider();

            containerBuilder.Populate(serviceCollection);

            containerBuilder.Register(c => new OptionsWrapper<UserDataWebSocketClientOptions>(new UserDataWebSocketClientOptions
            {
                KeepAliveTimerPeriod = KEEP_ALIVE_PERIOD_IN_MILLISECONDS //If you set the default time(30 minutes) when the keep alive connection occurs unspecified error.
            }))
            .As<IOptions<UserDataWebSocketClientOptions>>();

            containerBuilder.RegisterType<CoinigyConfig>().SingleInstance();
            containerBuilder.RegisterType<TelegramConfig>().SingleInstance();
            containerBuilder.RegisterType<BittrexConfig>().SingleInstance();
            containerBuilder.RegisterType<BinanceConfig>().SingleInstance();
            containerBuilder.RegisterType<PoloniexConfig>().SingleInstance();
            containerBuilder.RegisterType<GeneralConfig>().SingleInstance();
            containerBuilder.RegisterType<CoinigyApiService>();
            containerBuilder.RegisterType<CryptoCompareApiService>();
            containerBuilder.RegisterType<BittrexService>();
            containerBuilder.RegisterType<PoloniexService>();
            containerBuilder.RegisterType<BinanceService>();
            containerBuilder.RegisterType<BinanceCacheService>().As<IBinanceCacheService>();
            containerBuilder.RegisterType<BinanceSubscriberService>().As<IBinanceSubscriberService>().SingleInstance();
            containerBuilder.RegisterType<BinanceWebsocketService>().As<IBinanceWebsocketService>().SingleInstance();
            containerBuilder.RegisterType<DatabaseService>();
            containerBuilder.RegisterType<TelegramMessageReceiveService>().SingleInstance();
            containerBuilder.RegisterType<TelegramMessageSendingService>();
            containerBuilder.RegisterType<StartupCheckingService>().SingleInstance();
            containerBuilder.RegisterType<CoinigyBalanceService>();
            containerBuilder.RegisterType<TelegramBot>().SingleInstance();
            containerBuilder.RegisterType<PriceService>();
            containerBuilder.RegisterType<ProfitAndLossService>();
            containerBuilder.RegisterType<TradeExportService>();
            containerBuilder.RegisterType<TelegramBittrexFileUploadService>();
            containerBuilder.RegisterType<TelegramPairProfitService>();
            containerBuilder.RegisterType<PoloniexClientFactory>().As<IPoloniexClientFactory>();

            CheckWhatIsEnabled(out bool coinigyEnabled,
                out bool bittrexEnabled,
                out bool poloniexEnabled,
                out bool binanceEnabled);

            var busBuilder = new BusBuilder();

            busBuilder.ConfigureCore(coinigyEnabled, bittrexEnabled, poloniexEnabled, binanceEnabled);

            containerBuilder.RegisterMicroBus(busBuilder);
            Container = containerBuilder.Build();

            var loggerFactory = Container.Resolve<ILoggerFactory>();
            var log = loggerFactory.CreateLogger<Program>();

            log.LogInformation($"Services\nCoinigy: {coinigyEnabled}\nBinance: {binanceEnabled}\nBittrex: {bittrexEnabled}\nPoloniex: {poloniexEnabled}");
            ConfigureConfig(Container, Configuration, log);
        } 

        #endregion

        #region Methods

        private static void ConfigureConfig(IContainer container, IConfiguration configuration, ILogger<Program> log)
        {
            try
            {
                var config = container.Resolve<GeneralConfig>();
                configuration.GetSection("General").Bind(config);
                if (!config.IsValid())
                {
                    throw new ApplicationException("General config is invalid - please check contents!");
                }
                log.LogInformation("Created General config");
            }
            catch (Exception)
            {
                log.LogError("Error in reading General config");
                throw;
            }

            try
            {
                var config = container.Resolve<TelegramConfig>();
                configuration.GetSection("Telegram").Bind(config);
                if (!config.IsValid())
                {
                    throw new ApplicationException("Telegram config is invalid - please check contents!");
                }
                log.LogInformation("Created Telegram config");
            }
            catch (Exception)
            {
                log.LogError("Error in reading Telegram config");
                throw;
            }

            try
            {
                var config = container.Resolve<CoinigyConfig>();
                configuration.GetSection("Coinigy").Bind(config);
                if (!config.IsValid())
                {
                    throw new ApplicationException("Coinigy config is invalid - please check contents!");
                }
                log.LogInformation("Created Coinigy config");
            }
            catch (Exception)
            {
                log.LogError("Error in reading Coinigy config");
                throw;
            }

            try
            {
                var config = container.Resolve<BinanceConfig>();
                configuration.GetSection("Binance").Bind(config);
                if (!config.IsValid())
                {
                    throw new ApplicationException("Binance config is invalid - please check contents!");
                }
                log.LogInformation("Created Binance config");
            }
            catch (Exception)
            {
                log.LogError("Error in reading Binance config");
                throw;
            }

            try
            {
                var config = container.Resolve<BittrexConfig>();
                configuration.GetSection("Bittrex").Bind(config);
                if (!config.IsValid())
                {
                    throw new ApplicationException("Bittrex config is invalid - please check contents!");
                }
                log.LogInformation("Created Bittrex Config");
            }
            catch (Exception)
            {
                log.LogError("Error in reading Bittrex config");
                throw;
            }

            try
            {
                var config = container.Resolve<PoloniexConfig>();
                configuration.GetSection("Poloniex").Bind(config);
                if (!config.IsValid())
                {
                    throw new ApplicationException("Poloniex config is invalid - please check contents!");
                }
                log.LogInformation("Created Poloniex config");
            }
            catch (Exception)
            {
                log.LogError("Error in reading Poloniex config");
                throw;
            }
        }

        private void CheckWhatIsEnabled(
            out bool coinigyEnabled,
            out bool bittrexEnabled,
            out bool poloniexEnabled,
            out bool binanceEnabled)
        {
            string coinigyEnabledString = Configuration.GetSection("Coinigy").GetValue("Enabled", "false");
            string binanceEnabledString = Configuration.GetSection("Binance").GetValue("Enabled", "false");
            string bittrexEnabledString = Configuration.GetSection("Bittrex").GetValue("Enabled", "false");
            string poloniexEnabledString = Configuration.GetSection("Poloniex").GetValue("Enabled", "false");

            coinigyEnabled = bool.Parse(coinigyEnabledString);
            binanceEnabled = bool.Parse(binanceEnabledString);
            bittrexEnabled = bool.Parse(bittrexEnabledString);
            poloniexEnabled = bool.Parse(poloniexEnabledString);
        }

        private async Task OnStarting()
        {
            var startupService = Container.Resolve<StartupCheckingService>();
            var context = Container.Resolve<CryptoGramBotDbContext>();

            await DbInitializer.Initialize(context);

            var limiterTotal = new RateLimiterPerEndpoint(1, TimeSpan.FromSeconds(1));
            var limiterPerEndpoint = new RateLimiterPerEndpoint(1, TimeSpan.FromSeconds(1));

            BittrexClient.SetDefaultOptions(new BittrexClientOptions
            {
                RateLimiters = new List<IRateLimiter>
                {
                    limiterTotal,
                    limiterPerEndpoint
                }
            });

            startupService.Start();
        } 

        #endregion
    }
}