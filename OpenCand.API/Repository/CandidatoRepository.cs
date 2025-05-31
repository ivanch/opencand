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
                    SELECT idcandidato, cpf, nome, datanascimento, email, sexo, estadocivil, escolaridade, ocupacao
                    FROM candidato
                    WHERE nome ILIKE '%' || @query || '%' OR
                          cpf ILIKE '%' || @query || '%' OR
                          email ILIKE '%' || @query || '%'
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
