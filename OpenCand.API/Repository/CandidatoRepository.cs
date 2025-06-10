using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using OpenCand.Core.Models;

namespace OpenCand.Repository
{
    public class CandidatoRepository : BaseRepository
    {
        public CandidatoRepository(IConfiguration configuration, IMemoryCache? cache = null) : base(configuration, cache)
        {
        }

        public async Task<List<Candidato>?> SearchCandidatosAsync(string query)
        {
            string cacheKey = GenerateCacheKey("Search", query);

            return await GetOrSetCacheAsync(cacheKey, async () =>
            {
                using var connection = new NpgsqlConnection(ConnectionString);
                return (await connection.QueryAsync<Candidato>(@"
                    SELECT c.*,
                        GREATEST(similarity(c.apelido, @q), similarity(c.nome, @q)) AS sim
                    FROM candidato c
                    WHERE c.apelido  % @q
                    OR c.nome     % @q
                    ORDER BY c.popularidade DESC, sim DESC, length(c.nome) ASC
                    LIMIT 10;
                ", new { q = query })).AsList();
            });
            
        }

        public async Task<Candidato?> GetCandidatoAsync(Guid idcandidato)
        {
            string cacheKey = GenerateCacheKey("Candidato", idcandidato);
            
            return await GetOrSetCacheAsync(cacheKey, async () =>
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    return await connection.QueryFirstOrDefaultAsync<Candidato>(@"
                        SELECT * FROM candidato
                        WHERE idcandidato = @idcandidato;",
                        new { idcandidato });
                }
            });
        }

        public async Task<string?> GetCandidatoCpfAsync(Guid idcandidato)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<string>(@"
                    SELECT cpf FROM candidato
                    WHERE idcandidato = @idcandidato;",
                    new { idcandidato });
            }
        }

        public async Task<List<CandidatoMapping>?> GetCandidatoMappingById(Guid idcandidato)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                var query = @"
                    SELECT * FROM candidato_mapping
                    WHERE idcandidato = @idcandidato";
                return (await connection.QueryAsync<CandidatoMapping>(query, new { idcandidato })).AsList();
            }
        }

        public async Task<Partido?> GetPartidoBySigla(string sigla)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                var query = @"
                    SELECT * FROM partido
                    WHERE sigla = @sigla";
                return await connection.QueryFirstOrDefaultAsync<Partido>(query, new { sigla });
            }
        }

        public async Task<List<RedeSocial>?> GetCandidatoRedeSocialById(Guid idcandidato)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                var query = @"
                    SELECT * FROM rede_social
                    WHERE idcandidato = @idcandidato";
                return (await connection.QueryAsync<RedeSocial>(query, new { idcandidato })).AsList();
            }
        }
    }
}
