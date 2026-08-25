using JarvisBot.Core.Models;

namespace JarvisBot.Core.Interfaces
{
    public interface IMonitoringResultRepository
    {
        Task AddAsync(MonitoringResult result, CancellationToken cancellationToken = default);

        Task<MonitoringResult?> GetLastAsync(Guid taskId, CancellationToken cancellationToken = default);

        Task<bool> WasConditionMetAsync(Guid taskId, CancellationToken cancellationToken = default);
    }
}
