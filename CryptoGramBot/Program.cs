using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Microsoft.AspNetCore.Hosting;

namespace CryptoGramBot
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                var configurationBuilder = new ConfigurationBuilder();
                configurationBuilder.SetBasePath(Path.Combine(Directory.GetCurrentDirectory()));
                configurationBuilder.AddJsonFile("appsettings.json", false, true);

                var config = configurationBuilder.Build();

                Log.Logger = new LoggerConfiguration()
                    .WriteTo.RollingFile(Directory.GetCurrentDirectory() + "/logs/CryptoGramBot.log")
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .CreateLogger();

                var serverUrls = config.GetSection("General").GetValue<string>("ServerUrls");

                var webHost = new WebHostBuilder()
                    .UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseStartup<Startup>()
                    .UseConfiguration(config)
                    .UseSerilog()
                    .UseUrls(serverUrls)
                    .Build();

                webHost.Run();
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}