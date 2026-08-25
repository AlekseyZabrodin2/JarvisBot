namespace JarvisBot.Core.Models
{
    public sealed class MonitoringResult
    {
        public Guid TaskId { get; init; }

        public DateTimeOffset CheckedAt { get; init; }

        public bool IsSuccess { get; init; }

        public bool ConditionMet { get; init; }

        public string? Value { get; init; }

        public string? Error { get; init; }
    }
}
