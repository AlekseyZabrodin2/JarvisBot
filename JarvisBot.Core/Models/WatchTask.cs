using JarvisBot.Core.Enums;

namespace JarvisBot.Core.Models
{
    public sealed class WatchTask
    {
        public Guid Id { get; init; }

        public string Name { get; set; } = string.Empty;

        public Uri Url { get; set; } = null!;

        public TimeSpan Interval { get; set; }

        public bool IsEnabled { get; set; }

        public ConditionType ConditionType { get; init; }

        public string ConditionValue { get; init; } = string.Empty;

        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    }
}
