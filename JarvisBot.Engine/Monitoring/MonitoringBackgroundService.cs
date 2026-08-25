using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;

namespace JarvisBot.Engine.Monitoring
{
    public sealed class MonitoringBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public MonitoringBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Info("Monitoring service started.");

            try
            {
                using var scope = _scopeFactory.CreateScope();

                var monitoringService = scope.ServiceProvider.GetRequiredService<MonitoringService>();

                await monitoringService.RunAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                _logger.Info("Monitoring service stopped.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Monitoring service terminated unexpectedly.");
            }
        }
    }
}
