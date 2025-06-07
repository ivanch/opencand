using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OpenCand.Core.Models;

namespace OpenCand.Repository
{
    public class DespesaReceitaRepository : BaseRepository
    {
        public DespesaReceitaRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<List<Despesa>> GetDespesasByCandidatoIdYearAsync(Guid idcandidato)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                return (await connection.QueryAsync<Despesa>(@"
                    SELECT * FROM despesas_candidato
                    WHERE idcandidato = @idcandidato
                    ORDER BY valor DESC;", new
                {
                    idcandidato
                })).AsList();
            }
        }

        public async Task<List<Receita>> GetReceitasByCandidatoIdYearAsync(Guid idcandidato)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                return (await connection.QueryAsync<Receita>(@"
                    SELECT * FROM receitas_candidato
                    WHERE idcandidato = @idcandidato
                    ORDER BY valor DESC;", new
                {
                    idcandidato
                })).AsList();
            }
        }
    }
}
