using JarvisBot.Core.Interfaces;
using JarvisBot.Core.Models;
using NLog;

namespace JarvisBot.Engine.Monitoring
{
    public sealed class MonitoringTaskRunner
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IPageMonitor _pageMonitor;
        private readonly IMonitoringResultRepository _resultRepository; 
        private readonly INotificationService _notificationService;


        public MonitoringTaskRunner(IPageMonitor pageMonitor, IMonitoringResultRepository resultRepository, INotificationService notificationService)
        {
            _pageMonitor = pageMonitor;
            _resultRepository = resultRepository;
            _notificationService = notificationService;
        }


        public async Task RunAsync(WatchTask task, CancellationToken cancellationToken = default)
        {
            var previousResult = await _resultRepository.GetLastAsync(task.Id, cancellationToken);

            var result = await _pageMonitor.CheckAsync(task, cancellationToken);

            var conditionTriggered =
                result.IsSuccess &&
                result.ConditionMet &&
                previousResult?.ConditionMet != true;

            _logger.Info(
                "Monitoring result. " +
                "TaskId=[{TaskId}], " +
                "IsSuccess=[{IsSuccess}], " +
                "ConditionMet=[{ConditionMet}], " +
                "Value=[{Value}], " +
                "Error=[{Error}]",
                result.TaskId,
                result.IsSuccess,
                result.ConditionMet,
                result.Value,
                result.Error);

            await _resultRepository.AddAsync(result, cancellationToken);

            if (conditionTriggered)
            {
                _logger.Info("Condition triggered for task [{TaskId}].", task.Id);

                await _notificationService.NotifyAsync(task, result, cancellationToken);
            }
        }
    }
}
