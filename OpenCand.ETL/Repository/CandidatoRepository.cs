using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OpenCand.Core.Models;

namespace OpenCand.Repository
{
    public class CandidatoRepository : BaseRepository
    {
        public CandidatoRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task AddCandidatoAsync(Candidato candidato)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO candidato (idcandidato, cpf, nome, datanascimento, email, sexo, estadocivil, escolaridade, ocupacao)
                    VALUES (@idcandidato, @cpf, @nome, @datanascimento, @email, @sexo, @estadocivil, @escolaridade, @ocupacao)
                    ON CONFLICT (idcandidato) DO UPDATE SET
                        cpf = EXCLUDED.cpf,
                        nome = EXCLUDED.nome,
                        datanascimento = EXCLUDED.datanascimento,
                        email = EXCLUDED.email,
                        sexo = EXCLUDED.sexo,
                        estadocivil = EXCLUDED.estadocivil,
                        escolaridade = EXCLUDED.escolaridade,
                        ocupacao = EXCLUDED.ocupacao;",
                    new
                    {
                        idcandidato = candidato.IdCandidato,
                        cpf = candidato.Cpf,
                        nome = candidato.Nome,
                        datanascimento = candidato.DataNascimento,
                        email = candidato.Email,
                        sexo = candidato.Sexo,
                        estadocivil = candidato.EstadoCivil,
                        escolaridade = candidato.Escolaridade,
                        ocupacao = candidato.Ocupacao
                    });
            }
        }

        public async Task AddCandidatoMappingAsync(CandidatoMapping candidatoMapping)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO candidato_mapping (idcandidato, cpf, nome, sqcandidato, ano, tipoeleicao, siglauf, nomeue, cargo, nrcandidato, resultado)
                    VALUES (@idcandidato, @cpf, @nome, @sqcandidato, @ano, @tipoeleicao, @siglauf, @nomeue, @cargo, @nrcandidato, @resultado);",
                    new
                    {
                        idcandidato = candidatoMapping.IdCandidato,
                        cpf = candidatoMapping.Cpf,
                        nome = candidatoMapping.Nome,
                        sqcandidato = candidatoMapping.SqCandidato,
                        ano = candidatoMapping.Ano,
                        tipoeleicao = candidatoMapping.TipoEleicao,
                        siglauf = candidatoMapping.SiglaUF,
                        nomeue = candidatoMapping.NomeUE,
                        nrcandidato = candidatoMapping.NrCandidato,
                        cargo = candidatoMapping.Cargo,
                        resultado = candidatoMapping.Resultado
                    });
            }
        }

        public async Task<List<CandidatoMapping>?> GetCandidatoMappingByCpf(string cpf)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                var query = @"
                    SELECT idcandidato, cpf, nome, sqcandidato, ano, tipoeleicao, siglauf, nomeue, cargo, nrcandidato, resultado
                    FROM candidato_mapping
                    WHERE cpf = @cpf";
                return (await connection.QueryAsync<CandidatoMapping>(query, new { cpf })).AsList();
            }
        }

        public async Task<List<CandidatoMapping>?> GetCandidatoMappingByNome(string nome)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                var query = @"
                    SELECT idcandidato, cpf, nome, sqcandidato, ano, tipoeleicao, siglauf, nomeue, cargo, nrcandidato, resultado
                    FROM candidato_mapping
                    WHERE nome = @nome";
                return (await connection.QueryAsync<CandidatoMapping>(query, new { nome })).AsList();
            }
        }

        public async Task<CandidatoMapping?> GetIdCandidatoBySqCandidato(string sqCandidato, int ano, string siglauf, string nomeue)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                var query = @"
                    SELECT idcandidato
                    FROM candidato_mapping
                    WHERE sqcandidato = @sqCandidato AND 
                          ano = @ano AND
                          siglauf = @siglauf AND
                          nomeue = @nomeue";
                return await connection.QueryFirstOrDefaultAsync<CandidatoMapping>(query, new { sqCandidato, ano, siglauf, nomeue });
            }
        }

        public async Task<CandidatoMapping?> GetIdCandidatoBySqCandidato(string sqCandidato, int ano, string siglauf)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                var query = @"
                    SELECT idcandidato
                    FROM candidato_mapping
                    WHERE sqcandidato = @sqCandidato AND 
                          ano = @ano AND
                          siglauf = @siglauf";
                return await connection.QueryFirstOrDefaultAsync<CandidatoMapping>(query, new { sqCandidato, ano, siglauf });
            }
        }
    }
}
