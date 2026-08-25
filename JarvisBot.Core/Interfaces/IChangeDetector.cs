using JarvisBot.Core.Models;

namespace JarvisBot.Core.Interfaces
{
    public interface IChangeDetector
    {
        bool HasChanged(MonitoringResult? previous, MonitoringResult current);
    }
}
