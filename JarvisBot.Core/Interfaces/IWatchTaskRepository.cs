using JarvisBot.Core.Models;

namespace JarvisBot.Core.Interfaces
{
    public interface IWatchTaskRepository
    {
        Task<IReadOnlyList<WatchTask>> GetEnabledAsync(CancellationToken cancellationToken = default);

        Task<List<WatchTask>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<WatchTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task AddAsync(WatchTask task, CancellationToken cancellationToken = default);

        Task UpdateAsync(WatchTask task, CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
