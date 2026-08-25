using JarvisBot.Core.Models;

namespace JarvisBot.Core.Interfaces
{
    public interface IPageMonitor
    {
        Task<MonitoringResult> CheckAsync(WatchTask task, CancellationToken cancellationToken = default);
    }
}
