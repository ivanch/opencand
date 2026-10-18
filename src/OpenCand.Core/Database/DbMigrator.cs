using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace OpenCand.Core.Database
{
    /// <summary>
    /// Applies the scripts in db/migrations (embedded in this assembly) that are not yet recorded in schema_migrations.
    /// Run on startup by both the API and the ETL; an advisory lock makes concurrent starts wait for each other.
    /// Each migration runs in its own transaction (scripts must not contain BEGIN/COMMIT). When anything was applied,
    /// db/mv.sql is re-run in the last migration's transaction so the materialized views match the new schema.
    /// </summary>
    public static class DbMigrator
    {
        // Arbitrary, fixed key for pg_advisory_lock ("OCMIGRAT").
        private const long LockKey = 0x4F434D4947524154;

        private const string MigrationPrefix = "migrations/";

        internal sealed record Migration(string Version, string Sql, string Checksum);

        public static async Task<int> MigrateAsync(NpgsqlDataSource dataSource, ILogger logger, CancellationToken ct = default)
        {
            var migrations = LoadMigrations();

            await using var connection = await dataSource.OpenConnectionAsync(ct);

            await ExecuteAsync(connection, null, "SELECT pg_advisory_lock($1)", ct, LockKey);
            try
            {
                await ExecuteAsync(connection, null, """
                    CREATE TABLE IF NOT EXISTS schema_migrations (
                        version TEXT PRIMARY KEY,
                        checksum TEXT NOT NULL,
                        applied_at TIMESTAMPTZ NOT NULL DEFAULT now()
                    )
                    """, ct);

                var applied = await GetAppliedAsync(connection, ct);

                foreach (var migration in migrations.Where(m => applied.ContainsKey(m.Version)))
                {
                    if (applied[migration.Version] != migration.Checksum)
                        logger.LogWarning("Migration {Version} was changed after it was applied", migration.Version);
                }

                var pending = migrations.Where(m => !applied.ContainsKey(m.Version)).ToList();
                if (pending.Count == 0)
                {
                    logger.LogInformation("Database schema is up to date ({Count} migrations applied)", applied.Count);
                    return 0;
                }

                for (var i = 0; i < pending.Count; i++)
                {
                    var migration = pending[i];
                    logger.LogInformation("Applying migration {Version}", migration.Version);

                    await using var tx = await connection.BeginTransactionAsync(ct);
                    await ExecuteAsync(connection, tx, migration.Sql, ct);
                    await ExecuteAsync(connection, tx, "INSERT INTO schema_migrations (version, checksum) VALUES ($1, $2)", ct,
                        migration.Version, migration.Checksum);

                    if (i == pending.Count - 1)
                    {
                        logger.LogInformation("Recreating materialized views");
                        await ExecuteAsync(connection, tx, LoadResource("mv.sql"), ct);
                    }

                    await tx.CommitAsync(ct);
                }

                logger.LogInformation("Applied {Count} migration(s)", pending.Count);
                return pending.Count;
            }
            finally
            {
                // Not ct: the lock must be released even when startup is being cancelled.
                await ExecuteAsync(connection, null, "SELECT pg_advisory_unlock($1)", CancellationToken.None, LockKey);
            }
        }

        internal static IReadOnlyList<Migration> LoadMigrations()
        {
            return typeof(DbMigrator).Assembly.GetManifestResourceNames()
                .Where(name => name.StartsWith(MigrationPrefix, StringComparison.Ordinal) && name.EndsWith(".sql", StringComparison.Ordinal))
                .Order(StringComparer.Ordinal)
                .Select(name =>
                {
                    var sql = LoadResource(name);
                    var checksum = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sql)));
                    return new Migration(Path.GetFileNameWithoutExtension(name[MigrationPrefix.Length..]), sql, checksum);
                })
                .ToList();
        }

        /// <summary>Reads an SQL script embedded from db/ (mv.sql, migrations/NNN_name.sql).</summary>
        public static string LoadResource(string name)
        {
            using var stream = typeof(DbMigrator).Assembly.GetManifestResourceStream(name)
                ?? throw new InvalidOperationException($"The embedded resource {name} was not found.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private static async Task<Dictionary<string, string>> GetAppliedAsync(NpgsqlConnection connection, CancellationToken ct)
        {
            await using var command = new NpgsqlCommand("SELECT version, checksum FROM schema_migrations", connection);
            await using var reader = await command.ExecuteReaderAsync(ct);
            var applied = new Dictionary<string, string>();
            while (await reader.ReadAsync(ct)) applied[reader.GetString(0)] = reader.GetString(1);
            return applied;
        }

        private static async Task ExecuteAsync(NpgsqlConnection connection, NpgsqlTransaction? tx, string sql, CancellationToken ct, params object[] parameters)
        {
            await using var command = new NpgsqlCommand(sql, connection, tx) { CommandTimeout = 0 };
            foreach (var parameter in parameters)
                command.Parameters.Add(new NpgsqlParameter { Value = parameter });
            await command.ExecuteNonQueryAsync(ct);
        }
    }
}
