using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using OpenCand.Config;
using OpenCand.ETL.Database;
using OpenCand.ETL.Parsing;

namespace OpenCand.ETL.Loading
{
    /// <summary>
    /// Streams the rows of every CSV file straight into "{table}_new" with binary COPY (several files in parallel, one
    /// connection each). Nothing is accumulated in memory and the live table is not touched: the swap happens later.
    /// </summary>
    public abstract class StreamingFactLoader<TCsv, TRow>
    {
        private readonly NpgsqlDataSource dataSource;
        private readonly SqIndex sqIndex;
        private readonly EtlSettings settings;
        private readonly ILogger logger;

        protected StreamingFactLoader(NpgsqlDataSource dataSource, SqIndex sqIndex, IOptions<EtlSettings> settings, ILogger logger)
        {
            this.dataSource = dataSource;
            this.sqIndex = sqIndex;
            this.settings = settings.Value;
            this.logger = logger;
        }

        public abstract string Dataset { get; }
        public abstract SwapTable Table { get; }
        protected abstract string CopyCommand { get; }

        protected abstract bool TryPrepare(TCsv csv, Guid idCandidato, out TRow row, out string reason);
        protected abstract void Write(NpgsqlBinaryImporter writer, TRow row);
        protected abstract int Ano(TCsv csv);
        protected abstract string? Sq(TCsv csv);

        /// <summary>Returns stats with Files == 0 (and no "_new" table) when there is nothing to load.</summary>
        public async Task<LoadStats> LoadAsync(string directory, CancellationToken ct)
        {
            var stats = new LoadStats(Dataset);
            if (!Directory.Exists(directory))
            {
                logger.LogWarning("{Dataset}: directory not found at {Directory}, skipping (the current table is kept)", Dataset, directory);
                return stats;
            }

            var files = CsvSource.ListFiles(directory, logger);
            stats.Files = files.Count;
            if (files.Count == 0)
            {
                logger.LogWarning("{Dataset}: no CSV files in {Directory}, skipping (the current table is kept)", Dataset, directory);
                return stats;
            }

            await sqIndex.EnsureLoadedAsync(ct);
            await Table.PrepareAsync(dataSource, ct);

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(1, settings.MaxParallelFiles),
                CancellationToken = ct
            };

            await Parallel.ForEachAsync(files, options, (file, token) =>
                new ValueTask(Task.Run(() => LoadFile(file, stats, token), token)));

            return stats;
        }

        private void LoadFile(string file, LoadStats stats, CancellationToken ct)
        {
            var encoding = Encoding.GetEncoding(settings.CsvEncoding);
            long rows = 0;

            using var connection = dataSource.OpenConnection();
            using var writer = connection.BeginBinaryImport(CopyCommand);

            foreach (var csv in CsvSource.Read<TCsv>(file, encoding, stats, logger))
            {
                if (++rows % 10_000 == 0) ct.ThrowIfCancellationRequested();

                var sq = Sq(csv);
                if (sq is null || !sqIndex.TryGet(Ano(csv), sq, out var idCandidato))
                {
                    stats.Reject(sq is null ? "missing_field" : "candidate_not_found");
                    continue;
                }

                // everything that can fail happens before StartRow, so an importer row is never left half written
                if (!TryPrepare(csv, idCandidato, out var row, out var reason))
                {
                    stats.Reject(reason);
                    continue;
                }

                writer.StartRow();
                Write(writer, row);
                stats.AddWritten();
            }

            writer.Complete();
        }
    }
}
