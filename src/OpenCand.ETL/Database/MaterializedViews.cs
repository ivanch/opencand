using Microsoft.Extensions.Logging;
using Npgsql;
using OpenCand.Core.Database;

namespace OpenCand.ETL.Database
{
    /// <summary>
    /// Swaps the rebuilt tables in and (re)builds the materialized views. mv.sql is embedded from db/mv.sql so the ETL
    /// and the schema scripts never drift apart.
    /// </summary>
    public sealed class MaterializedViews
    {
        // Dependency order: views built on other views come after them (refreshed in this order, dropped in reverse).
        private static readonly string[] ViewNames =
        {
            "mv_bem_candidato", "mv_bem_candidato_ano", "mv_despesas_candidato", "mv_receitas_candidato", "mv_candidato_mapping_analytics",
            "mv_bem_resumo", "mv_despesas_resumo", "mv_receitas_resumo", "mv_enriquecimento_candidato"
        };

        private readonly NpgsqlDataSource dataSource;
        private readonly ILogger<MaterializedViews> logger;

        public MaterializedViews(NpgsqlDataSource dataSource, ILogger<MaterializedViews> logger)
        {
            this.dataSource = dataSource;
            this.logger = logger;
        }

        public static string LoadSql() => DbMigrator.LoadResource("mv.sql");

        /// <summary>
        /// With swaps: one transaction drops the views, swaps the tables and recreates the views, so readers see either the
        /// old or the new data. Without swaps the views are refreshed CONCURRENTLY (readers are not blocked), unless a
        /// rebuild is requested or a view is missing.
        /// </summary>
        public async Task CutoverAsync(IReadOnlyCollection<SwapTable> swaps, bool recreateViews, CancellationToken ct)
        {
            if (swaps.Count > 0 || recreateViews || !await AllViewsExistAsync(ct))
            {
                await using var connection = await dataSource.OpenConnectionAsync(ct);
                await using var tx = await connection.BeginTransactionAsync(ct);

                foreach (var view in Enumerable.Reverse(ViewNames))
                    await connection.ExecuteAsync($"DROP MATERIALIZED VIEW IF EXISTS {view}", ct, timeoutSeconds: 0);

                logger.LogInformation("Swapping in {Count} table(s) and recreating materialized views", swaps.Count);
                foreach (var swap in swaps)
                    foreach (var sql in swap.CutoverStatements())
                        await connection.ExecuteAsync(sql, ct, timeoutSeconds: 0);

                await connection.ExecuteAsync(LoadSql(), ct, timeoutSeconds: 0);

                await tx.CommitAsync(ct);
                return;
            }

            logger.LogInformation("Refreshing materialized views");
            foreach (var view in ViewNames)
            {
                await using var connection = await dataSource.OpenConnectionAsync(ct);
                await connection.ExecuteAsync($"REFRESH MATERIALIZED VIEW CONCURRENTLY {view}", ct, timeoutSeconds: 0);
                await connection.ExecuteAsync($"ANALYZE {view}", ct, timeoutSeconds: 0);
            }
        }

        private async Task<bool> AllViewsExistAsync(CancellationToken ct)
        {
            await using var command = dataSource.CreateCommand("SELECT matviewname FROM pg_matviews WHERE schemaname = current_schema()");
            await using var reader = await command.ExecuteReaderAsync(ct);
            var existing = new HashSet<string>();
            while (await reader.ReadAsync(ct)) existing.Add(reader.GetString(0));
            return ViewNames.All(existing.Contains);
        }
    }
}
