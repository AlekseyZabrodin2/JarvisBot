using JarvisBot.Background;
using JarvisBot.Data;
using JarvisBot.KeyboardButtons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace JarvisBot
{
    class JarvisMind
    {
                

        private static async Task Main(string[] args)
        {
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