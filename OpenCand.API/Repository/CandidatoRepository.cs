using Dapper;
using Npgsql;
using OpenCand.Core.Models;

namespace OpenCand.Repository
{
    public class CandidatoRepository : BaseRepository
    {
        public CandidatoRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<List<Candidato>> SearchCandidatosAsync(string query)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                return (await connection.QueryAsync<Candidato>(@"
                    SELECT *
                          CASE
                            WHEN lower(nome) = lower(@query) THEN 0             -- Exact match (case-insensitive)
                            WHEN lower(nome) LIKE lower(@query) || '%' THEN 1   -- Starts with match (case-insensitive)
                            WHEN lower(nome) LIKE '%' || lower(@query) THEN 2   -- Contains anywhere match (case-insensitive)
                            WHEN cpf = @query THEN 0                            -- Exact match for CPF
                            WHEN cpf LIKE @query || '%' THEN 1                  -- Starts with match for CPF
                            WHEN cpf LIKE '%' || @query THEN 2                  -- Contains anywhere match for CPF
                            ELSE 3
                          END AS name_rank
                    FROM candidato
                    WHERE nome ILIKE '%' || @query || '%' OR
                          cpf ILIKE '%' || @query || '%'
                    ORDER BY name_rank,
                            length(nome) ASC
                    LIMIT 10;",
                    new { query })).AsList();
            }
        }

        public async Task<Candidato?> GetCandidatoAsync(Guid idcandidato)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<Candidato>(@"
                    SELECT * FROM candidato
                    WHERE idcandidato = @idcandidato;",
                    new { idcandidato });
            }
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
