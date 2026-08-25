using JarvisBot.Core.Interfaces;
using JarvisBot.Storage.DataBase;
using JarvisBot.Storage.Initialization;
using JarvisBot.Storage.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JarvisBot.Storage.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJarvisBotStorage(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<JarvisBotDbContext>(options => options.UseNpgsql(connectionString));
            services.AddScoped<DatabaseInitializer>(); 
            services.AddScoped<IWatchTaskRepository, WatchTaskRepository>();
            services.AddScoped<IMonitoringResultRepository, MonitoringResultRepository>();

            return services;
        }
    }
}
