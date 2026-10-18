using Npgsql;
using NpgsqlTypes;

namespace OpenCand.ETL.Database
{
    /// <summary>Typed helpers for NpgsqlBinaryImporter (binary COPY).</summary>
    public static class PgWriter
    {
        private static long truncated;

        /// <summary>Number of text values clipped to the column length since the process started.</summary>
        public static long TruncatedValues => Interlocked.Read(ref truncated);

        public static void WriteText(this NpgsqlBinaryImporter writer, string? value, int maxLength = 0)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            if (maxLength > 0 && value.Length > maxLength)
            {
                Interlocked.Increment(ref truncated);
                value = value[..maxLength];
            }

            writer.Write(value, NpgsqlDbType.Text);
        }

        public static void WriteGuid(this NpgsqlBinaryImporter writer, Guid value) => writer.Write(value, NpgsqlDbType.Uuid);

        public static void WriteInt(this NpgsqlBinaryImporter writer, int value) => writer.Write(value, NpgsqlDbType.Integer);

        public static void WriteDate(this NpgsqlBinaryImporter writer, DateOnly? value)
        {
            if (value is null) writer.WriteNull();
            else writer.Write(value.Value, NpgsqlDbType.Date);
        }

        public static void WriteTimestampUtc(this NpgsqlBinaryImporter writer, DateTime? value)
        {
            if (value is null) writer.WriteNull();
            else writer.Write(value.Value, NpgsqlDbType.TimestampTz);
        }

        public static void WriteNumeric(this NpgsqlBinaryImporter writer, decimal? value)
        {
            if (value is null) writer.WriteNull();
            else writer.Write(value.Value, NpgsqlDbType.Numeric);
        }

        public static async Task ExecuteAsync(this NpgsqlConnection connection, string sql, CancellationToken ct = default, int timeoutSeconds = -1)
        {
            await using var command = new NpgsqlCommand(sql, connection);
            if (timeoutSeconds >= 0) command.CommandTimeout = timeoutSeconds;
            await command.ExecuteNonQueryAsync(ct);
        }
    }
}
