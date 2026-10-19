using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using OpenCand.Config;
using OpenCand.ETL.Database;
using OpenCand.ETL.Loading;

namespace OpenCand.ETL.Pipeline
{
    /// <summary>
    /// candidatos -> bens -> redes sociais (incremental upserts), despesas/receitas (rebuilt into "_new" tables), then one
    /// cutover that swaps the tables and rebuilds the views. Any failure ends the run with a non-zero exit code; the live
    /// despesas/receitas tables are only replaced after every file loaded within the allowed reject ratio.
    /// </summary>
    public sealed class EtlRunner
    {
        public static readonly string[] Datasets = { "candidatos", "bens", "redes", "despesas", "receitas" };

        private readonly IConfiguration configuration;
        private readonly CsvSettings csv;
        private readonly EtlSettings settings;
        private readonly NpgsqlDataSource dataSource;
        private readonly SqIndex sqIndex;
        private readonly CandidatoLoader candidatoLoader;
        private readonly BemCandidatoLoader bemLoader;
        private readonly RedeSocialLoader redeLoader;
        private readonly DespesaLoader despesaLoader;
        private readonly ReceitaLoader receitaLoader;
        private readonly MaterializedViews views;
        private readonly EtlRunLog runLog;
        private readonly ILogger<EtlRunner> logger;

        public EtlRunner(
            IConfiguration configuration, IOptions<CsvSettings> csv, IOptions<EtlSettings> settings, NpgsqlDataSource dataSource,
            SqIndex sqIndex, CandidatoLoader candidatoLoader, BemCandidatoLoader bemLoader, RedeSocialLoader redeLoader,
            DespesaLoader despesaLoader, ReceitaLoader receitaLoader, MaterializedViews views, EtlRunLog runLog, ILogger<EtlRunner> logger)
        {
            this.configuration = configuration;
            this.csv = csv.Value;
            this.settings = settings.Value;
            this.dataSource = dataSource;
            this.sqIndex = sqIndex;
            this.candidatoLoader = candidatoLoader;
            this.bemLoader = bemLoader;
            this.redeLoader = redeLoader;
            this.despesaLoader = despesaLoader;
            this.receitaLoader = receitaLoader;
            this.views = views;
            this.runLog = runLog;
            this.logger = logger;
        }

        /// <returns>The process exit code: 0 on success, 1 on failure, 2 on invalid arguments.</returns>
        public async Task<int> RunAsync(CancellationToken ct)
        {
            var selected = ParseSelection(configuration["only"]);
            if (selected is null) return 2;

            var basePath = ResolveBasePath();
            logger.LogInformation("ETL starting. BasePath={BasePath}, datasets={Datasets}, parallel files={Files}, batch size={Batch}",
                basePath, string.Join(",", selected), settings.MaxParallelFiles, settings.BatchSize);

            var total = Stopwatch.StartNew();
            var startedAt = DateTime.UtcNow;
            var results = new List<LoadStats>();
            var swaps = new List<SwapTable>();
            var swapCandidates = new List<SwapTable>();
            var succeeded = false;

            try
            {
                if (selected.Contains("candidatos"))
                {
                    var stats = await candidatoLoader.LoadAsync(Path.Combine(basePath, csv.CandidatosFolder), ct);
                    results.Add(stats);
                    stats.EnsureWithin(settings.MaxRejectRatio);
                    sqIndex.Invalidate();
                }

                if (selected.Contains("bens"))
                {
                    var stats = await bemLoader.LoadAsync(Path.Combine(basePath, csv.BensCandidatosFolder), ct);
                    results.Add(stats);
                    stats.EnsureWithin(settings.MaxRejectRatio);
                }

                if (selected.Contains("redes"))
                {
                    var stats = await redeLoader.LoadAsync(Path.Combine(basePath, csv.RedesSociaisFolder), ct);
                    results.Add(stats);
                    stats.EnsureWithin(settings.MaxRejectRatio);
                }

                if (selected.Contains("despesas"))
                    await LoadFactAsync(despesaLoader, Path.Combine(basePath, csv.DespesaCandidatoFolder), swaps, swapCandidates, results, ct);

                if (selected.Contains("receitas"))
                    await LoadFactAsync(receitaLoader, Path.Combine(basePath, csv.ReceitaCandidatoFolder), swaps, swapCandidates, results, ct);

                await views.CutoverAsync(swaps, settings.RebuildViews, ct);
                succeeded = true;
            }
            catch (OperationCanceledException)
            {
                logger.LogError("ETL cancelled");
                return 1;
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "ETL failed. The current despesas/receitas tables were not replaced.");
                return 1;
            }
            finally
            {
                foreach (var summary in results) logger.LogInformation("{Summary}", summary.Summary());
                if (PgWriter.TruncatedValues > 0)
                    logger.LogWarning("{Count} text values were longer than their column and were truncated", PgWriter.TruncatedValues);

                if (!succeeded)
                {
                    foreach (var table in swapCandidates)
                    {
                        try { await table.DropNewAsync(dataSource); }
                        catch (Exception ex) { logger.LogWarning(ex, "Could not drop {Table}", table.NewTable); }
                    }
                }

                try { await runLog.RecordAsync(startedAt, succeeded ? "success" : "failed"); }
                catch (Exception ex) { logger.LogWarning(ex, "Could not write the etl_run row"); }
            }

            logger.LogInformation("ETL finished in {Elapsed}", total.Elapsed);
            return 0;
        }

        private async Task LoadFactAsync<TCsv, TRow>(
            StreamingFactLoader<TCsv, TRow> loader, string directory, List<SwapTable> swaps, List<SwapTable> swapCandidates,
            List<LoadStats> results, CancellationToken ct)
        {
            swapCandidates.Add(loader.Table);
            var stats = await loader.LoadAsync(directory, ct);
            results.Add(stats);

            if (stats.Files == 0) return; // nothing to load: keep the current table

            stats.EnsureWithin(settings.MaxRejectRatio);
            if (stats.Written == 0)
                throw new EtlException($"{loader.Dataset}: no row could be loaded; refusing to replace the current table with an empty one.");

            await loader.Table.BuildIndexesAsync(dataSource, ct);
            swaps.Add(loader.Table);
        }

        private HashSet<string>? ParseSelection(string? only)
        {
            if (string.IsNullOrWhiteSpace(only)) return new HashSet<string>(Datasets);

            var selected = only.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => s.ToLowerInvariant()).ToHashSet();
            var unknown = selected.Where(s => !Datasets.Contains(s)).ToList();
            if (unknown.Count > 0)
            {
                logger.LogError("Unknown dataset(s) in --only: {Unknown}. Valid: {Valid}", string.Join(",", unknown), string.Join(",", Datasets));
                return null;
            }

            return selected;
        }

        private string ResolveBasePath()
        {
            var basePath = configuration.GetValue<string>("BasePath");
            if (string.IsNullOrEmpty(basePath))
                throw new EtlException("BasePath is not configured.");

            if (Directory.Exists(basePath)) return basePath;

            var candidates = new[]
            {
                Path.GetFullPath(basePath),
                Path.Combine(Directory.GetCurrentDirectory(), basePath),
                Path.Combine(AppContext.BaseDirectory, basePath),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../sample")),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../sample")),
                Path.Combine(Directory.GetCurrentDirectory(), "sample"),
            };

            return candidates.FirstOrDefault(Directory.Exists) ?? basePath;
        }
    }
}
