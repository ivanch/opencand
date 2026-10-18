using Npgsql;

namespace OpenCand.ETL.Database
{
    /// <summary>
    /// A big fact table that is rebuilt from scratch: rows are COPYed into "{table}_new" (no indexes), indexes and the
    /// foreign key are built afterwards, then <see cref="Cutover"/> swaps it in. Names follow db/db.sql
    /// (pk_{table}, fk_{table}_candidato, idx_{table}_{column}).
    /// </summary>
    public sealed record SwapTable(string Table, string IdColumn, string[] IndexColumns)
    {
        public string NewTable => $"{Table}_new";

        public static readonly SwapTable Despesas = new("despesas_candidato", "iddespesa", new[] { "idcandidato", "ano", "sqcandidato", "sgpartido" });
        public static readonly SwapTable Receitas = new("receitas_candidato", "idreceita", new[] { "idcandidato", "ano", "sqcandidato", "sgpartido" });

        public async Task PrepareAsync(NpgsqlDataSource dataSource, CancellationToken ct)
        {
            await using var connection = await dataSource.OpenConnectionAsync(ct);
            await connection.ExecuteAsync($"DROP TABLE IF EXISTS {NewTable}", ct);
            await connection.ExecuteAsync($"CREATE TABLE {NewTable} (LIKE {Table} INCLUDING DEFAULTS)", ct);
        }

        public async Task DropNewAsync(NpgsqlDataSource dataSource)
        {
            await using var connection = await dataSource.OpenConnectionAsync();
            await connection.ExecuteAsync($"DROP TABLE IF EXISTS {NewTable}");
        }

        /// <summary>Builds the primary key, indexes and foreign key on the new table (concurrently) and analyzes it.</summary>
        public async Task BuildIndexesAsync(NpgsqlDataSource dataSource, CancellationToken ct)
        {
            var statements = new List<string>
            {
                $"ALTER TABLE {NewTable} ADD CONSTRAINT pk_{Table}_new PRIMARY KEY ({IdColumn})",
                $"ALTER TABLE {NewTable} ADD CONSTRAINT fk_{Table}_candidato_new FOREIGN KEY (idcandidato) " +
                "REFERENCES candidato(idcandidato) ON DELETE CASCADE ON UPDATE CASCADE"
            };
            statements.AddRange(IndexColumns.Select(c => $"CREATE INDEX idx_{Table}_{c}_new ON {NewTable} ({c})"));

            // each statement on its own connection: Postgres builds indexes of the same table in parallel
            await Task.WhenAll(statements.Select(async sql =>
            {
                await using var connection = await dataSource.OpenConnectionAsync(ct);
                await connection.ExecuteAsync(sql, ct, timeoutSeconds: 0);
            }));

            await using var analyze = await dataSource.OpenConnectionAsync(ct);
            await analyze.ExecuteAsync($"ANALYZE {NewTable}", ct, timeoutSeconds: 0);
        }

        /// <summary>SQL for the swap; must run in the same transaction as the dropping of the dependent materialized views.</summary>
        public IEnumerable<string> CutoverStatements()
        {
            yield return $"DROP TABLE {Table}";
            yield return $"ALTER TABLE {NewTable} RENAME TO {Table}";
            yield return $"ALTER TABLE {Table} RENAME CONSTRAINT pk_{Table}_new TO pk_{Table}";
            yield return $"ALTER TABLE {Table} RENAME CONSTRAINT fk_{Table}_candidato_new TO fk_{Table}_candidato";
            foreach (var column in IndexColumns)
                yield return $"ALTER INDEX idx_{Table}_{column}_new RENAME TO idx_{Table}_{column}";
        }
    }
}
