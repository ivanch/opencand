using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OpenCand.Core.Models;

namespace OpenCand.Repository
{
    public class BemCandidatoRepository : BaseRepository
    {
        public BemCandidatoRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task AddBemCandidatoAsync(BemCandidato bemCandidato)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO bem_candidato (idcandidato, ano, ordembem, tipobem, descricao, valor)
                    VALUES (@idcandidato, @ano, @ordembem, @tipobem, @descricao, @valor)
                    ON CONFLICT (idcandidato, ano, ordembem) DO UPDATE SET
                        tipobem = EXCLUDED.tipobem,
                        descricao = EXCLUDED.descricao,
                        valor = EXCLUDED.valor;",
                new
                {
                    idcandidato = bemCandidato.IdCandidato,
                    ano = bemCandidato.Ano,
                    ordembem = bemCandidato.OrdemBem,
                    tipobem = bemCandidato.TipoBem,
                    descricao = bemCandidato.Descricao,
                    valor = bemCandidato.Valor
                });
            }
        }
    }
}
