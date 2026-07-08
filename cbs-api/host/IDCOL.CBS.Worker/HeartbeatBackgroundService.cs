namespace IDCOL.CBS.Worker;

/// <summary>
/// Phase 0 placeholder proving the Worker host runs and can be scheduled/monitored. Real jobs
/// (classification batch, schedule-recompute sweep, CL-1..7 regulatory export, invoice sweep)
/// land in their corresponding phases per the architecture plan - this class intentionally
/// does no business work yet.
/// </summary>
public class HeartbeatBackgroundService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    private readonly ILogger<HeartbeatBackgroundService> _logger;

    public HeartbeatBackgroundService(ILogger<HeartbeatBackgroundService> logger) => _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("IDCOL.CBS.Worker heartbeat at {TimeUtc}", DateTime.UtcNow);
            await Task.Delay(Interval, stoppingToken);
        }
    }
}
