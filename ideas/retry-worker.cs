using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

public class HourlyReaderWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<HourlyReaderWorker> _logger;

    public HourlyReaderWorker(IServiceProvider services, ILogger<HourlyReaderWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run immediately, then every hour.
        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));
        try
        {
            // First run immediately:
            await DoWorkAsync(stoppingToken);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DoWorkAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Worker cancellation requested.");
        }
    }

    private async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("HourlyReaderWorker started at: {time}", DateTimeOffset.UtcNow);

        // create a scope so DbContext is scoped per run
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // If your table is small: simple read:
        // var all = await db.MyEntities.AsNoTracking().ToListAsync(cancellationToken);
        // foreach (var e in all) { /* process */ }

        // For bigger tables: stream with AsAsyncEnumerable to avoid materializing everything:
        await foreach (var entity in db.MyEntities
                                         .AsNoTracking()
                                         .AsAsyncEnumerable()
                                         .WithCancellation(cancellationToken))
        {
            // process each entity
            // consider projecting to a lightweight DTO to reduce transfer size
        }

        _logger.LogInformation("HourlyReaderWorker finished at: {time}", DateTimeOffset.UtcNow);
    }
}
