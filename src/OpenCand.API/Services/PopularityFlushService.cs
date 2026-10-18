using OpenCand.Repository;

namespace OpenCand.API.Services
{
    /// <summary>Writes the view counts collected by <see cref="PopularityTracker"/> to the database every 30 seconds.</summary>
    public class PopularityFlushService : BackgroundService
    {
        internal static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(30);

        private readonly PopularityTracker tracker;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<PopularityFlushService> logger;

        public PopularityFlushService(PopularityTracker tracker, IServiceProvider serviceProvider, ILogger<PopularityFlushService> logger)
        {
            this.tracker = tracker;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(FlushInterval);

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await FlushAsync();
                }
            }
            catch (OperationCanceledException)
            {
                // shutting down
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            await FlushAsync(); // do not lose the last interval's views
        }

        internal async Task FlushAsync()
        {
            var (ids, counts) = tracker.Drain();
            if (ids.Length == 0)
                return;

            try
            {
                using var scope = serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<CandidatoRepository>();
                await repository.AddPopularityAsync(ids, counts);
                logger.LogDebug("Flushed popularity for {Count} candidatos", ids.Length);
            }
            catch (Exception ex)
            {
                tracker.Restore(ids, counts);
                logger.LogError(ex, "Failed to flush popularity for {Count} candidatos; will retry", ids.Length);
            }
        }
    }
}
