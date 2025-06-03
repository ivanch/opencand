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
                    VALUES (@idcandidato, @cpf, @nome, @apelido, @sqcandidato, @ano, @tipoeleicao, @siglauf, @nomeue, @cargo, @nrcandidato, @sgpartido, @resultado)
                    ON CONFLICT DO NOTHING;",
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

        public async Task<Candidato?> GetCandidatoByCpf(string cpf)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                var query = @"
                    SELECT *
                    FROM candidato
                    WHERE cpf = @cpf";
                return await connection.QueryFirstOrDefaultAsync<Candidato>(query, new { cpf });
            }
        }

        public async Task<Candidato?> GetCandidatoByNome(string nome, DateTime datanascimento)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                var query = @"
                    SELECT *
                    FROM candidato
                    WHERE nome = @nome AND datanascimento = @datanascimento";
                return await connection.QueryFirstOrDefaultAsync<Candidato>(query, new { nome, datanascimento });
            }
        }

        public async Task<CandidatoMapping?> GetCandidatoMappingByDetails(string nome, int ano, string cargo, string siglauf, string nomeue, string nrcandidato, string resultado)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                var query = @"
                    SELECT *
                    FROM candidato_mapping
                    WHERE nome = @nome AND 
                          ano = @ano AND 
                          cargo = @cargo AND 
                          siglauf = @siglauf AND 
                          nomeue = @nomeue AND 
                          nrcandidato = @nrcandidato AND 
                          resultado = @resultado";
                return await connection.QueryFirstOrDefaultAsync<CandidatoMapping>(query, new { nome, ano, cargo, siglauf, nomeue, nrcandidato, resultado });
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
