using JarvisBot.Background;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JarvisBot
{
    class JarvisMind
    {

        private static ILogger _logger = LogManager.GetCurrentClassLogger();


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
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<JarvisBackgroundService>();
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