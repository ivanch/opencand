using Microsoft.Extensions.Caching.Memory;
using OpenCand.API.Repository;

namespace OpenCand.API.Services
{
    /// <summary>
    /// Polls etl_run for a new run. When the latest run id changes, every cached entry is dropped and the
    /// preload is run again so the API serves the freshly loaded data.
    /// </summary>
    public class DataVersionWatcher : BackgroundService
    {
        internal static readonly TimeSpan DefaultPollInterval = TimeSpan.FromMinutes(5);

        private readonly TimeSpan pollInterval;

        private readonly IServiceProvider serviceProvider;
        private readonly IMemoryCache cache;
        private readonly CachePreloadService preloadService;
        private readonly ILogger<DataVersionWatcher> logger;

        private int? lastVersion;
        private bool initialized;

        public DataVersionWatcher(
            IServiceProvider serviceProvider,
            IMemoryCache cache,
            IConfiguration configuration,
            CachePreloadService preloadService,
            ILogger<DataVersionWatcher> logger)
        {
            this.serviceProvider = serviceProvider;
            this.cache = cache;
            this.preloadService = preloadService;
            this.logger = logger;

            var seconds = configuration.GetValue<int?>("CacheSettings:VersionPollSeconds");
            pollInterval = seconds > 0 ? TimeSpan.FromSeconds(seconds.Value) : DefaultPollInterval;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(pollInterval);

            try
            {
                // First poll only records the current version: CachePreloadService does the initial preload.
                await CheckAsync(stoppingToken);
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await CheckAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // shutting down
            }
        }

        internal async Task CheckAsync(CancellationToken cancellationToken)
        {
            int? version;
            try
            {
                using var scope = serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<OpenCandRepository>();
                version = await repository.GetLatestEtlRunIdAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not read the data version from etl_run");
                return;
            }

            if (!initialized)
            {
                initialized = true;
                lastVersion = version;
                return;
            }

            if (version == lastVersion)
                return;

            logger.LogInformation("Data version changed from {Old} to {New}; clearing cache", lastVersion, version);
            lastVersion = version;

            (cache as MemoryCache)?.Compact(1.0);
            await preloadService.PreloadAsync(cancellationToken);
        }
    }
}
