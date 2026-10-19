using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using OpenCand.Config;
using OpenCand.ETL.Database;
using OpenCand.ETL.Parsing;
using OpenCand.Parser.Models;

namespace OpenCand.ETL.Loading
{
    /// <summary>Loads rede_social files with a temp-table COPY + upsert per batch.</summary>
    public sealed class RedeSocialLoader
    {
        private sealed record RedeRow(Guid Id, int Ano, string Link, string Rede, string? SiglaUf);

        private readonly NpgsqlDataSource dataSource;
        private readonly SqIndex sqIndex;
        private readonly EtlSettings settings;
        private readonly ILogger<RedeSocialLoader> logger;

        public RedeSocialLoader(NpgsqlDataSource dataSource, SqIndex sqIndex, IOptions<EtlSettings> settings, ILogger<RedeSocialLoader> logger)
        {
            this.dataSource = dataSource;
            this.sqIndex = sqIndex;
            this.settings = settings.Value;
            this.logger = logger;
        }

        public async Task<LoadStats> LoadAsync(string directory, CancellationToken ct)
        {
            var stats = new LoadStats("redes_sociais");
            if (!Directory.Exists(directory))
            {
                logger.LogWarning("Redes sociais: directory not found at {Directory}, skipping", directory);
                return stats;
            }

            var files = CsvSource.ListFiles(directory, logger);
            stats.Files = files.Count;
            if (files.Count == 0) return stats;

            await sqIndex.EnsureLoadedAsync(ct);
            var encoding = Encoding.GetEncoding(settings.CsvEncoding);

            foreach (var file in files)
            {
                ct.ThrowIfCancellationRequested();

                // (idcandidato, ano, link) is the primary key
                var pending = new Dictionary<(Guid, int, string), RedeRow>();

                foreach (var row in CsvSource.Read<RedeSocialCSV>(file, encoding, stats, logger))
                {
                    var sq = TseParsing.NullIfBlank(row.SequencialCandidato);
                    if (sq is null || !sqIndex.TryGet(row.DataEleicao, sq, out var id))
                    {
                        stats.Reject("candidate_not_found");
                        continue;
                    }

                    var link = TseParsing.NormalizeUrl(row.Url);
                    if (link is null)
                    {
                        stats.Reject("invalid_url");
                        continue;
                    }

                    pending[(id, row.DataEleicao, link)] = new RedeRow(id, row.DataEleicao, link, TseParsing.ClassifyRede(link), TseParsing.NullIfBlank(row.SiglaUF));
                    stats.AddWritten();

                    if (pending.Count >= settings.BatchSize)
                    {
                        await FlushAsync(pending.Values, ct);
                        pending.Clear();
                    }
                }

                await FlushAsync(pending.Values, ct);
            }

            return stats;
        }

        private async Task FlushAsync(IEnumerable<RedeRow> rows, CancellationToken ct)
        {
            var list = rows.ToList();
            if (list.Count == 0) return;

            await using var connection = await dataSource.OpenConnectionAsync(ct);
            await using var tx = await connection.BeginTransactionAsync(ct);

            await connection.ExecuteAsync("CREATE TEMP TABLE stg_rede (LIKE rede_social INCLUDING DEFAULTS) ON COMMIT DROP", ct);
            await using (var w = await connection.BeginBinaryImportAsync(
                "COPY stg_rede (idcandidato, rede, siglauf, ano, link) FROM STDIN (FORMAT BINARY)", ct))
            {
                foreach (var r in list)
                {
                    w.StartRow();
                    w.WriteGuid(r.Id);
                    w.WriteText(r.Rede, 50);
                    w.WriteText(r.SiglaUf, 2);
                    w.WriteInt(r.Ano);
                    w.WriteText(r.Link);
                }
                await w.CompleteAsync(ct);
            }

            await connection.ExecuteAsync(@"
                INSERT INTO rede_social (idcandidato, rede, siglauf, ano, link)
                SELECT idcandidato, rede, siglauf, ano, link FROM stg_rede
                ON CONFLICT (idcandidato, ano, link) DO UPDATE SET rede = EXCLUDED.rede, siglauf = EXCLUDED.siglauf
                WHERE (rede_social.rede, rede_social.siglauf) IS DISTINCT FROM (EXCLUDED.rede, EXCLUDED.siglauf);", ct);

            await tx.CommitAsync(ct);
        }
    }
}
