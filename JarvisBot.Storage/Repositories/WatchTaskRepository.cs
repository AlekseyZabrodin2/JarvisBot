using JarvisBot.Core.Interfaces;
using JarvisBot.Core.Models;
using JarvisBot.Storage.DataBase;
using Microsoft.EntityFrameworkCore;

namespace JarvisBot.Storage.Repositories
{
    public sealed class WatchTaskRepository : IWatchTaskRepository
    {
        private readonly JarvisBotDbContext _dbContext;

        public WatchTaskRepository(JarvisBotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<WatchTask>> GetEnabledAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.WatchTasks
                .AsNoTracking()
                .Where(x => x.IsEnabled)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<WatchTask>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.WatchTasks
                .AsNoTracking()
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<WatchTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.WatchTasks
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task AddAsync(WatchTask task, CancellationToken cancellationToken = default)
        {
            await _dbContext.WatchTasks.AddAsync(task, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(WatchTask task, CancellationToken cancellationToken = default)
        {
            _dbContext.WatchTasks.Update(task);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _dbContext.WatchTasks
                .Where(x => x.Id == id)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
