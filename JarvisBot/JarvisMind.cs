using JarvisBot.Background;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JarvisBot
{
    class JarvisMind
    {
                

        private static async Task Main(string[] args)
        {
            using (Mutex mutex = new Mutex(true, "MyApp", out bool isNewInstance))
            {
                using var tocen = new CancellationTokenSource();

                if (!isNewInstance)
                {
                    Console.WriteLine("Программа запущена !!!");
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

                await host.RunAsync();

            }
        }

    }
}