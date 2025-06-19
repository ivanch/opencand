using System;
using Microsoft.Extensions.Logging;
using OpenCand.API.Model;

namespace OpenCand.API.Services
{
    public class CachePreloadService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<CachePreloadService> logger;

        public CachePreloadService(IServiceProvider serviceProvider, ILogger<CachePreloadService> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Wait a bit for the application to fully start up
            await Task.Delay(5000, stoppingToken);
            logger.LogInformation("Starting cache preload process...");

            using var scope = serviceProvider.CreateScope();
            var estatisticaService = scope.ServiceProvider.GetRequiredService<EstatisticaService>();
            var openCandService = scope.ServiceProvider.GetRequiredService<OpenCandService>();

            // First, preload single-call endpoints
            await PreloadSingleEndpoints(estatisticaService, openCandService);

            await PerformPreLoadEstatistica(estatisticaService);

            logger.LogInformation("Cache preload process completed.");
        }

        private async Task PerformPreLoadEstatistica(EstatisticaService estatisticaService)
        {
            await PerformPreLoad("GetConfigurationModel", estatisticaService.GetConfigurationModel);

            var types = new[] { "bem", "despesa", "receita" };
            var groupByValues = new[] { "candidato", "partido", "uf", "cargo" };

            logger.LogInformation($"Preloading cache with GetValueSum requests (3 types * 4 groupBy combinations)");

            foreach (var type in types)
            {
                foreach (var groupBy in groupByValues)
                {
                    var request = new GetValueSumRequest
                    {
                        Type = type,
                        GroupBy = groupBy,
                        Filter = null // No filters as requested
                    };

                    logger.LogDebug($"Executing cache preload request: Type={type}, GroupBy={groupBy}");

                    await PerformPreLoad("GetValueSum", () => estatisticaService.GetValueSum(request));
                }
            }
        }

        private async Task PreloadSingleEndpoints(EstatisticaService estatisticaService, OpenCandService openCandService)
        {
            logger.LogInformation("Preloading single-call endpoints...");

            await PerformPreLoad("GetOpenCandStatsAsync", estatisticaService.GetMaioresEnriquecimentos);
            await PerformPreLoad("GetOpenCandStatsAsync", openCandService.GetOpenCandStatsAsync);
            await PerformPreLoad("GetDataAvailabilityStatsAsync", openCandService.GetDataAvailabilityStatsAsync);

            logger.LogInformation("Single-call endpoints preload completed");
        }

        private async Task PerformPreLoad(string name, Func<Task> action)
        {
            try
            {
                logger.LogDebug($"Executing cache preload for {name}");

                var startTime = DateTime.UtcNow;
                await action();
                var duration = DateTime.UtcNow - startTime;

                logger.LogInformation($"Cache preload completed for {name}: Duration={duration.TotalMilliseconds}ms");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to perform preload action for {name}");
            }
            finally
            {
                await Task.Delay(100); // Small delay to avoid overwhelming the database
            }
        }
    }
}
