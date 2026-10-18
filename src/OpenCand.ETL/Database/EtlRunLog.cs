using Npgsql;

namespace OpenCand.ETL.Database
{
    /// <summary>Writes one etl_run row per finished run; the API watches max(id) to know when to drop its caches.</summary>
    public sealed class EtlRunLog
    {
        private readonly NpgsqlDataSource dataSource;

        public EtlRunLog(NpgsqlDataSource dataSource)
        {
            this.dataSource = dataSource;
        }

        public async Task RecordAsync(DateTime startedAtUtc, string status)
        {
            await using var command = dataSource.CreateCommand(
                "INSERT INTO etl_run (started_at, finished_at, status) VALUES ($1, now(), $2)");
            command.Parameters.AddWithValue(NpgsqlTypes.NpgsqlDbType.TimestampTz, startedAtUtc);
            command.Parameters.AddWithValue(NpgsqlTypes.NpgsqlDbType.Text, status);
            await command.ExecuteNonQueryAsync();
        }
    }
}
