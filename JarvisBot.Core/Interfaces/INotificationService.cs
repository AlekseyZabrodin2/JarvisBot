using JarvisBot.Core.Models;

namespace JarvisBot.Core.Interfaces
{
    public interface INotificationService
    {
        Task NotifyAsync(WatchTask task, MonitoringResult result, CancellationToken cancellationToken = default);
    }
}
