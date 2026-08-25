using System;

namespace JarvisBot.Models
{
    public sealed class MonitoringCreationState
    {
        public string? Name { get; set; }

        public string? Url { get; set; }

        public string? ConditionValue { get; set; }

        public TimeSpan? Interval { get; set; }

        public MonitoringCreationStep Step { get; set; }
    }
}
