using System.Collections.Concurrent;

namespace Inventa.Infrastructure;

public sealed class PerformanceMetrics
{
    private static readonly Lazy<PerformanceMetrics> _instance = new(() => new PerformanceMetrics());
    public static PerformanceMetrics Instance => _instance.Value;

    private readonly ConcurrentDictionary<string, MetricsEntry> _metrics = new();

    private PerformanceMetrics() { }

    public void RecordUcLoad(string ucName, long elapsedMs, bool fromCache)
    {
        _metrics.AddOrUpdate($"uc_{ucName}",
            _ => new MetricsEntry { TotalCalls = 1, TotalMs = elapsedMs },
            (_, existing) =>
            {
                existing.TotalCalls++;
                existing.TotalMs += elapsedMs;
                return existing;
            });
    }

    public void RecordTabSwitch(string tabName, long elapsedMs)
    {
        _metrics.AddOrUpdate($"tab_{tabName}",
            _ => new MetricsEntry { TotalCalls = 1, TotalMs = elapsedMs },
            (_, existing) =>
            {
                existing.TotalCalls++;
                existing.TotalMs += elapsedMs;
                return existing;
            });
    }

    private class MetricsEntry
    {
        public long TotalCalls { get; set; }
        public long TotalMs { get; set; }
    }
}
