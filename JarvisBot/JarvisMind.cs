using JarvisBot.Background;
using JarvisBot.Exchange.AlfaBankInSyncRates;
using JarvisBot.Weather;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JarvisBot
{
    class JarvisMind
    {

        private static ILogger _logger = LogManager.GetCurrentClassLogger();

        public JarvisClientSettings JarvisClientSettings { get; set; }


        private static async Task Main(string[] args)
        {
            _logger.Info("Begine start Jarvis");

            using (Mutex mutex = new Mutex(true, "MyApp", out bool isNewInstance))
            {
                using var tocen = new CancellationTokenSource();

                if (!isNewInstance)
                {
                    _logger.Info("Попытка запустить второй экземпляр программы !!!");
                    Console.WriteLine("Попытка запустить второй экземпляр программы !!!");
                    return;
                }
                var host = Host
                .CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    var env = context.HostingEnvironment;

                    config.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "AppSettings/jarvisClientSettings.json"));

                    // add UserSecrets when Development
                    //if (env.IsDevelopment())
                    //{
                        config.AddUserSecrets<JarvisMind>();
                    //}

                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;

                    services.AddSingleton<CommunicationMethods>();
                    services.AddSingleton<JarvisBackgroundService>();
                    services.AddSingleton<ExchangeRateLoder>();
                    services.AddSingleton<OldExchangeRates>();
                    services.AddSingleton<WeatherLoder>();

                    services.AddHostedService<JarvisBackgroundService>();

                    services.Configure<JarvisClientSettings>(configuration.GetSection(nameof(JarvisClientSettings)));
                })
                .UseWindowsService()
                .Build();
                try
                {
                    _logger.Info("Begine RunAsync");

                    await host.RunAsync();
                }
                catch (Exception exeption)
                {
                    _logger.Error($"Jarvis don`t started{exeption}");
                    throw;
                }
            }
        }
    }
}