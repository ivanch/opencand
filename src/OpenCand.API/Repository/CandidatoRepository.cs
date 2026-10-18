using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using OpenCand.Core.Models;

namespace OpenCand.Repository
{
    /// <summary>A candidato with the rows the candidate page needs, loaded in one round trip.</summary>
    public class CandidatoDetail
    {
        public Candidato Candidato { get; set; } = null!;
        public List<CandidatoMapping> Eleicoes { get; set; } = new();
        public List<CandidatoExt> CandidatoExt { get; set; } = new();
    }

    public class CandidatoRepository : BaseRepository
    {
        // candidato_mapping row plus the columns of the LEFT JOINed partido.
        private class MappingRow : CandidatoMapping
        {
            public string? Partido_Sigla { get; set; }
            public string? Partido_Nome { get; set; }
            public int? Partido_Numero { get; set; }
        }

        // Old (merged) candidate ids keep working: they resolve to the canonical id through candidato_alias.
        private const string ResolvedId = "COALESCE((SELECT new_id FROM candidato_alias WHERE old_id = @idcandidato), @idcandidato)";

        public CandidatoRepository(IConfiguration configuration, IMemoryCache? cache = null) : base(configuration, cache)
        {
        }

        /// <summary>Trim + lower-case so "  Lula" and "lula" share one cache entry (and one query plan).</summary>
        internal static string NormalizeSearchQuery(string query) => query.Trim().ToLowerInvariant();

        internal static string SearchCacheKey(string normalizedQuery) => GenerateCacheKey("Search", normalizedQuery);

        public async Task<List<Candidato>?> SearchCandidatosAsync(string query)
        {
            var q = NormalizeSearchQuery(query);

            return await GetOrSetCacheAsync(SearchCacheKey(q), async () =>
            {
                using var connection = new NpgsqlConnection(ConnectionString);
                return (await connection.QueryAsync<Candidato>(@"
                    SELECT c.*,
                        GREATEST(similarity(c.apelido, @q), similarity(c.nome, @q)) AS sim
                    FROM candidato c
                    WHERE c.apelido  % @q
                    OR c.nome     % @q
                    ORDER BY sim DESC, c.popularidade DESC, length(c.nome) ASC
                    LIMIT 10;
                ", new { q })).AsList();
            }, SearchCacheExpiration);
        }

        /// <summary>
        /// Loads candidato, mappings (with partido) and ext in one round trip. When the id is not a candidato,
        /// it is looked up in candidato_alias and the canonical candidato is served instead.
        /// </summary>
        public async Task<CandidatoDetail?> GetCandidatoDetailAsync(Guid idcandidato)
        {
            using var connection = new NpgsqlConnection(ConnectionString);

            var detail = await LoadDetailAsync(connection, idcandidato);
            if (detail != null)
                return detail;

            var canonicalId = await connection.QueryFirstOrDefaultAsync<Guid?>(
                "SELECT new_id FROM candidato_alias WHERE old_id = @idcandidato;", new { idcandidato });

            return canonicalId.HasValue ? await LoadDetailAsync(connection, canonicalId.Value) : null;
        }

        private static async Task<CandidatoDetail?> LoadDetailAsync(NpgsqlConnection connection, Guid idcandidato)
        {
            using var grid = await connection.QueryMultipleAsync(@"
                SELECT * FROM candidato WHERE idcandidato = @idcandidato;

                SELECT cm.*, p.sigla AS partido_sigla, p.nome AS partido_nome, p.numero AS partido_numero
                FROM candidato_mapping cm
                LEFT JOIN partido p ON p.sigla = cm.sgpartido
                WHERE cm.idcandidato = @idcandidato;

                SELECT * FROM candidato_ext WHERE idcandidato = @idcandidato;",
                new { idcandidato });

            var candidato = await grid.ReadFirstOrDefaultAsync<Candidato>();
            var eleicoes = (await grid.ReadAsync<MappingRow>()).Select(row =>
            {
                row.Partido = row.Partido_Sigla == null
                    ? null
                    : new Partido { Sigla = row.Partido_Sigla, Nome = row.Partido_Nome ?? string.Empty, Numero = row.Partido_Numero ?? 0 };
                return (CandidatoMapping)row;
            }).ToList();
            var ext = (await grid.ReadAsync<CandidatoExt>()).AsList();

            return candidato == null
                ? null
                : new CandidatoDetail { Candidato = candidato, Eleicoes = eleicoes, CandidatoExt = ext };
        }

        public async Task<string?> GetCandidatoCpfAsync(Guid idcandidato)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<string>($@"
                    SELECT cpf FROM candidato
                    WHERE idcandidato = {ResolvedId};",
                    new { idcandidato });
            }
        }

        public async Task<List<RedeSocial>?> GetCandidatoRedeSocialById(Guid idcandidato)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                var query = $@"
                    SELECT * FROM rede_social
                    WHERE idcandidato = {ResolvedId}";
                return (await connection.QueryAsync<RedeSocial>(query, new { idcandidato })).AsList();
            }
        }

        /// <summary>Adds the given view counts to popularidade with a single statement.</summary>
        public async Task AddPopularityAsync(Guid[] ids, int[] counts)
        {
            if (ids.Length == 0)
                return;

            using var connection = new NpgsqlConnection(ConnectionString);
            await connection.ExecuteAsync(@"
                UPDATE candidato c
                SET popularidade = c.popularidade + d.cnt
                FROM unnest(@ids, @counts) AS d(id, cnt)
                WHERE c.idcandidato = d.id;
            ", new { ids, counts });
        }

        public async Task<Guid?> GetRandomCandidatoIdAsync()
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            // tsm_system_rows is installed by db/db.sql; SYSTEM_ROWS(1) always returns a row when the table is not empty.
            return await connection.QueryFirstOrDefaultAsync<Guid?>(@"
                SELECT idcandidato
                FROM candidato TABLESAMPLE SYSTEM_ROWS(1);
            ");
        }
    }
}
