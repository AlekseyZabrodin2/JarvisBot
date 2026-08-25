using JarvisBot.Core.Interfaces;
using JarvisBot.Core.Models;
using JarvisBot.Storage.DataBase;
using Microsoft.EntityFrameworkCore;

namespace JarvisBot.Storage.Repositories
{
    public sealed class MonitoringResultRepository : IMonitoringResultRepository
    {
        private readonly JarvisBotDbContext _dbContext;

        public MonitoringResultRepository(JarvisBotDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(MonitoringResult result, CancellationToken cancellationToken = default)
        {
            await _dbContext.MonitoringResults.AddAsync(result, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<MonitoringResult?> GetLastAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.MonitoringResults
                .AsNoTracking()
                .Where(x => x.TaskId == taskId)
                .OrderByDescending(x => x.CheckedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> WasConditionMetAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            var lastResult = await GetLastAsync(taskId, cancellationToken);

            return lastResult?.ConditionMet == true;
        }
    }
}
