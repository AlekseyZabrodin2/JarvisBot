using JarvisBot.Core.Interfaces;
using JarvisBot.Engine.Monitoring;
using JarvisBot.Engine.Playwright;
using Microsoft.Extensions.DependencyInjection;

namespace JarvisBot.Engine.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJarvisBotEngine(this IServiceCollection services)
        {
            services.AddSingleton<PlaywrightBrowser>();

            services.AddHostedService<PlaywrightBrowserHostedService>();

            services.AddScoped<IPageMonitor, PlaywrightPageMonitor>();
            services.AddScoped<MonitoringTaskRunner>();
            services.AddScoped<MonitoringService>();

            services.AddHostedService<MonitoringBackgroundService>();

            return services;
        }
    }
}
