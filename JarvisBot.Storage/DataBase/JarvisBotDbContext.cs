using JarvisBot.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace JarvisBot.Storage.DataBase
{
    public sealed class JarvisBotDbContext : DbContext
    {
        public JarvisBotDbContext(
            DbContextOptions<JarvisBotDbContext> options) 
            : base(options)
        {
        }

        public DbSet<WatchTask> WatchTasks => Set<WatchTask>();

        public DbSet<MonitoringResult> MonitoringResults => Set<MonitoringResult>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("jarvis");

            modelBuilder.Entity<WatchTask>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
                entity.Property(x => x.Url).HasConversion(url => url.ToString(), value => new Uri(value)).IsRequired().HasMaxLength(2048);
                entity.Property(x => x.Interval).HasConversion(interval => interval.TotalSeconds, seconds => TimeSpan.FromSeconds(seconds)).IsRequired();
                entity.Property(x => x.IsEnabled).IsRequired();
                entity.Property(x => x.ConditionType).HasConversion<int>().IsRequired();
                entity.Property(x => x.ConditionValue).IsRequired().HasMaxLength(2000); 
                entity.Property(x => x.CreatedAt).IsRequired();
            });

            modelBuilder.Entity<MonitoringResult>(entity =>
            {
                entity.HasKey(x => new { x.TaskId, x.CheckedAt });
                entity.Property(x => x.CheckedAt).IsRequired();
                entity.Property(x => x.IsSuccess).IsRequired();
                entity.Property(x => x.ConditionMet).IsRequired();
                entity.Property(x => x.Value).HasMaxLength(4000);
                entity.Property(x => x.Error).HasMaxLength(4000);
            });
        }
    }
}
