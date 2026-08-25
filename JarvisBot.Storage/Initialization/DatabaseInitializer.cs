using JarvisBot.Storage.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JarvisBot.Storage.Initialization
{
    public sealed class DatabaseInitializer
    {
        private readonly JarvisBotDbContext _dbContext;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(
            JarvisBotDbContext dbContext,
            ILogger<DatabaseInitializer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task InitializeAsync(
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Initializing JarvisBot database...");

            await _dbContext.Database.MigrateAsync(cancellationToken);

            _logger.LogInformation("JarvisBot database initialized.");
        }
    }
}
