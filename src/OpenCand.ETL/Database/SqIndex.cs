using Npgsql;

namespace OpenCand.ETL.Database
{
    /// <summary>
    /// In-memory (ano, sqcandidato) -> idcandidato lookup built from candidato_mapping, replacing one SELECT per CSV row.
    /// The sequential number is only unique within an election year, so the year is part of the key.
    /// </summary>
    public sealed class SqIndex
    {
        private readonly NpgsqlDataSource dataSource;
        private Dictionary<(int Ano, string Sq), Guid>? map;

        public SqIndex(NpgsqlDataSource dataSource)
        {
            this.dataSource = dataSource;
        }

        public int Count => map?.Count ?? 0;

        /// <summary>Call after candidato_mapping changed.</summary>
        public void Invalidate() => map = null;

        public async Task EnsureLoadedAsync(CancellationToken ct)
        {
            if (map is not null) return;

            var loaded = new Dictionary<(int, string), Guid>();
            await using var command = dataSource.CreateCommand("SELECT ano, sqcandidato, idcandidato FROM candidato_mapping");
            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                loaded.TryAdd((reader.GetInt32(0), reader.GetString(1)), reader.GetGuid(2));
            }

            map = loaded;
        }

        public bool TryGet(int ano, string sqCandidato, out Guid idCandidato)
        {
            if (map is null) throw new InvalidOperationException("SqIndex is not loaded.");
            return map.TryGetValue((ano, sqCandidato), out idCandidato);
        }
    }
}
