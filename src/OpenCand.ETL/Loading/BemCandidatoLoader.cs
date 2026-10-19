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
    /// <summary>Loads bem_candidato files with a temp-table COPY + upsert per batch.</summary>
    public sealed class BemCandidatoLoader
    {
        private sealed record BemRow(Guid Id, int Ano, int Ordem, string? Tipo, string? Descricao, decimal? Valor);

        private readonly NpgsqlDataSource dataSource;
        private readonly SqIndex sqIndex;
        private readonly EtlSettings settings;
        private readonly ILogger<BemCandidatoLoader> logger;

        public BemCandidatoLoader(NpgsqlDataSource dataSource, SqIndex sqIndex, IOptions<EtlSettings> settings, ILogger<BemCandidatoLoader> logger)
        {
            this.dataSource = dataSource;
            this.sqIndex = sqIndex;
            this.settings = settings.Value;
            this.logger = logger;
        }

        public async Task<LoadStats> LoadAsync(string directory, CancellationToken ct)
        {
            var stats = new LoadStats("bens");
            if (!Directory.Exists(directory))
            {
                logger.LogWarning("Bens: directory not found at {Directory}, skipping", directory);
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

                // (idcandidato, ano, ordembem) is the primary key: the last row wins inside a batch
                var pending = new Dictionary<(Guid, int, int), BemRow>();

                foreach (var row in CsvSource.Read<BemCandidatoCSV>(file, encoding, stats, logger))
                {
                    var sq = TseParsing.NullIfBlank(row.SequencialCandidato);
                    if (sq is null || !sqIndex.TryGet(row.AnoEleicao, sq, out var id))
                    {
                        stats.Reject("candidate_not_found");
                        continue;
                    }

                    if (!TseParsing.TryParseMoney(row.ValorBemCandidato, out var valor))
                    {
                        stats.Reject("invalid_valor");
                        continue;
                    }

                    pending[(id, row.AnoEleicao, row.NumeroOrdemBemCandidato)] = new BemRow(
                        id, row.AnoEleicao, row.NumeroOrdemBemCandidato,
                        TseParsing.NullIfBlank(row.DescricaoTipoBemCandidato), TseParsing.NullIfBlank(row.DescricaoBemCandidato), valor);
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

        private async Task FlushAsync(IEnumerable<BemRow> rows, CancellationToken ct)
        {
            var list = rows.ToList();
            if (list.Count == 0) return;

            await using var connection = await dataSource.OpenConnectionAsync(ct);
            await using var tx = await connection.BeginTransactionAsync(ct);

            await connection.ExecuteAsync("CREATE TEMP TABLE stg_bem (LIKE bem_candidato INCLUDING DEFAULTS) ON COMMIT DROP", ct);
            await using (var w = await connection.BeginBinaryImportAsync(
                "COPY stg_bem (idcandidato, ano, ordembem, tipobem, descricao, valor) FROM STDIN (FORMAT BINARY)", ct))
            {
                foreach (var b in list)
                {
                    w.StartRow();
                    w.WriteGuid(b.Id);
                    w.WriteInt(b.Ano);
                    w.WriteInt(b.Ordem);
                    w.WriteText(b.Tipo, 150);
                    w.WriteText(b.Descricao, 500);
                    w.WriteNumeric(b.Valor);
                }
                await w.CompleteAsync(ct);
            }

            await connection.ExecuteAsync(@"
                INSERT INTO bem_candidato (idcandidato, ano, ordembem, tipobem, descricao, valor)
                SELECT idcandidato, ano, ordembem, tipobem, descricao, valor FROM stg_bem
                ON CONFLICT (idcandidato, ano, ordembem) DO UPDATE SET
                    tipobem = EXCLUDED.tipobem, descricao = EXCLUDED.descricao, valor = EXCLUDED.valor
                WHERE (bem_candidato.tipobem, bem_candidato.descricao, bem_candidato.valor)
                      IS DISTINCT FROM (EXCLUDED.tipobem, EXCLUDED.descricao, EXCLUDED.valor);", ct);

            await tx.CommitAsync(ct);
        }
    }
}
