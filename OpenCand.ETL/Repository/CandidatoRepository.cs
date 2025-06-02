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
                    INSERT INTO candidato (idcandidato, cpf, nome, apelido, datanascimento, email, sexo, estadocivil, escolaridade, ocupacao)
                    VALUES (@idcandidato, @cpf, @nome, @apelido, @datanascimento, @email, @sexo, @estadocivil, @escolaridade, @ocupacao)
                    ON CONFLICT (idcandidato) DO UPDATE SET
                        cpf = EXCLUDED.cpf,
                        nome = EXCLUDED.nome,
                        datanascimento = EXCLUDED.datanascimento,
                        email = EXCLUDED.email,
                        sexo = EXCLUDED.sexo,
                        estadocivil = EXCLUDED.estadocivil,
                        escolaridade = EXCLUDED.escolaridade,
                        ocupacao = EXCLUDED.ocupacao,
                        apelido = EXCLUDED.apelido;",
                    new
                    {
                        idcandidato = candidato.IdCandidato,
                        cpf = candidato.Cpf,
                        nome = candidato.Nome,
                        apelido = candidato.Apelido,
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
                    INSERT INTO candidato_mapping (idcandidato, cpf, nome, apelido, sqcandidato, ano, tipoeleicao, siglauf, nomeue, cargo, nrcandidato, sgpartido, resultado)
                    VALUES (@idcandidato, @cpf, @nome, @apelido, @sqcandidato, @ano, @tipoeleicao, @siglauf, @nomeue, @cargo, @nrcandidato, @sgpartido, @resultado);",
                    new
                    {
                        idcandidato = candidatoMapping.IdCandidato,
                        cpf = candidatoMapping.Cpf,
                        nome = candidatoMapping.Nome,
                        apelido = candidatoMapping.Apelido,
                        sqcandidato = candidatoMapping.SqCandidato,
                        ano = candidatoMapping.Ano,
                        tipoeleicao = candidatoMapping.TipoEleicao,
                        siglauf = candidatoMapping.SiglaUF,
                        nomeue = candidatoMapping.NomeUE,
                        nrcandidato = candidatoMapping.NrCandidato,
                        cargo = candidatoMapping.Cargo,
                        sgpartido = candidatoMapping.Partido?.Sigla,
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

        public async Task<List<CandidatoMapping>?> GetCandidatoMappingByNome(string nome, string sqcandidato, int ano, string siglauf, string nomeue, string nrcandidato)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                var query = @"
                    SELECT idcandidato, cpf, nome, sqcandidato, ano, tipoeleicao, siglauf, nomeue, cargo, nrcandidato, resultado
                    FROM candidato_mapping
                    WHERE nome = @nome AND sqcandidato = @sqcandidato AND ano = @ano AND siglauf = @siglauf AND nomeue = @nomeue AND nrcandidato = @nrcandidato";
                return (await connection.QueryAsync<CandidatoMapping>(query, new { nome, sqcandidato, ano, siglauf, nomeue, nrcandidato })).AsList();
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
