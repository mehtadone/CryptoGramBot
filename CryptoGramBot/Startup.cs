using Autofac;
using Autofac.Extensions.DependencyInjection;
using Binance;
using Bittrex.Net;
using Bittrex.Net.RateLimiter;
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
using System.Threading.Tasks;

namespace CryptoGramBot
{
    public class Startup
    {
        #region Fields

        private readonly int KEEP_ALIVE_PERIOD_IN_MILLISECONDS = 60 * 1000;//900000;

        #endregion

        #region Properites

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
            containerBuilder.RegisterType<BittrexService>();
            containerBuilder.RegisterType<PoloniexService>();
            containerBuilder.RegisterType<BinanceService>();
            containerBuilder.RegisterType<BinanceCacheService>().As<IBinanceCacheService>();
            containerBuilder.RegisterType<BinanceSubscriberService>().As<IBinanceSubscriberService>().SingleInstance();
            containerBuilder.RegisterType<BinanceWebsocketService>().As<IBinanceWebsocketService>().SingleInstance();
            containerBuilder.RegisterType<DatabaseService>();
            containerBuilder.RegisterType<TelegramMessageRecieveService>().SingleInstance();
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

            log.LogInformation($"Services\nCoinigy: {coinigyEnabled}\nBittrex: {bittrexEnabled}\nBinance: {binanceEnabled}\nPoloniex: {poloniexEnabled}");
            ConfigureConfig(Container, Configuration, log);
        } 

        #endregion

        #region Methods

        private static void ConfigureConfig(IContainer container, IConfiguration configuration, ILogger<Program> log)
        {
            try
            {
                var config = container.Resolve<CoinigyConfig>();
                configuration.GetSection("Coinigy").Bind(config);
                log.LogInformation("Created coinigy config");
            }
            catch (Exception)
            {
                log.LogError("Error in reading coinigy config");
                throw;
            }

            try
            {
                var config = container.Resolve<GeneralConfig>();
                configuration.GetSection("General").Bind(config);
                log.LogInformation("Created general config");
            }
            catch (Exception)
            {
                log.LogError("Error in reading general config");
                throw;
            }

            try
            {
                var config = container.Resolve<TelegramConfig>();
                configuration.GetSection("Telegram").Bind(config);
                log.LogInformation("Created telegram config");
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
                var config = container.Resolve<BinanceConfig>();
                configuration.GetSection("Binance").Bind(config);
                log.LogInformation("Created binance config");
            }
            catch (Exception)
            {
                log.LogError("Error in reading binance config");
                throw;
            }

            try
            {
                var config = container.Resolve<PoloniexConfig>();
                configuration.GetSection("Poloniex").Bind(config);
                log.LogInformation("Created poloniex config");
            }
            catch (Exception)
            {
                log.LogError("Error in reading poloniex config");
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
            bittrexEnabled = bool.Parse(bittrexEnabledString);
            poloniexEnabled = bool.Parse(poloniexEnabledString);
            binanceEnabled = bool.Parse(binanceEnabledString);
        }

        private async Task OnStarting()
        {
            var startupService = Container.Resolve<StartupCheckingService>();
            var context = Container.Resolve<CryptoGramBotDbContext>();

            await DbInitializer.Initialize(context);

            var limiterTotal = new RateLimiterPerEndpoint(1, TimeSpan.FromSeconds(1));
            var limiterPerEndpoint = new RateLimiterPerEndpoint(1, TimeSpan.FromSeconds(1));

            BittrexDefaults.AddDefaultRateLimiter(limiterTotal);
            BittrexDefaults.AddDefaultRateLimiter(limiterPerEndpoint);

            startupService.Start();
        } 

        #endregion
    }
}