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
                    INSERT INTO candidato (idcandidato, cpf, nome, apelido, datanascimento, sexo, ultimoano, localidade)
                    VALUES (@idcandidato, @cpf, @nome, @apelido, @datanascimento, @sexo, @ultimoano, @localidade)
                    ON CONFLICT (idcandidato) DO UPDATE SET
                        cpf = EXCLUDED.cpf,
                        nome = EXCLUDED.nome,
                        apelido = EXCLUDED.apelido,
                        datanascimento = EXCLUDED.datanascimento,
                        sexo = EXCLUDED.sexo,
                        localidade = EXCLUDED.localidade,
                        ultimoano = EXCLUDED.ultimoano
                    WHERE candidato.ultimoano IS NULL OR EXCLUDED.ultimoano > candidato.ultimoano;",
                    new
                    {
                        idcandidato = candidato.IdCandidato,
                        cpf = candidato.Cpf,
                        nome = candidato.Nome,
                        apelido = candidato.Apelido,
                        datanascimento = candidato.DataNascimento,
                        sexo = candidato.Sexo,
                        localidade = candidato.Localidade,
                        ultimoano = candidato.Ultimoano
                    });
            }
        }

        public async Task AddCandidatoMappingAsync(CandidatoMapping candidatoMapping)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO candidato_mapping (idcandidato, cpf, nome, sqcandidato, ano, turno, tipoeleicao, siglauf, nomeue, cargo, nrcandidato, sgpartido, resultado)
                    VALUES (@idcandidato, @cpf, @nome, @sqcandidato, @ano, @turno, @tipoeleicao, @siglauf, @nomeue, @cargo, @nrcandidato, @sgpartido, @resultado)
                    ON CONFLICT DO NOTHING;",
                    new
                    {
                        idcandidato = candidatoMapping.IdCandidato,
                        cpf = candidatoMapping.Cpf,
                        nome = candidatoMapping.Nome,
                        sqcandidato = candidatoMapping.SqCandidato,
                        ano = candidatoMapping.Ano,
                        turno = candidatoMapping.Turno,
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

        public async Task AddCandidatoExtAsync(CandidatoExt candidatoExt)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO candidato_ext (idcandidato, ano, apelido, email, estadocivil, escolaridade, ocupacao)
                    VALUES (@idcandidato, @ano, @apelido, @email, @estadocivil, @escolaridade, @ocupacao)
                    ON CONFLICT (idcandidato, ano) DO UPDATE SET
                        apelido = EXCLUDED.apelido,
                        email = EXCLUDED.email,
                        estadocivil = EXCLUDED.estadocivil,
                        escolaridade = EXCLUDED.escolaridade,
                        ocupacao = EXCLUDED.ocupacao;",
                    new
                    {
                        idcandidato = candidatoExt.IdCandidato,
                        ano = candidatoExt.Ano,
                        apelido = candidatoExt.Apelido,
                        email = candidatoExt.Email,
                        estadocivil = candidatoExt.EstadoCivil,
                        escolaridade = candidatoExt.Escolaridade,
                        ocupacao = candidatoExt.Ocupacao
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

        public async Task<Guid?> GetIdCandidatoBySqCandidato(string sqcandidato)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                var query = @"
                    SELECT idcandidato
                    FROM candidato_mapping
                    WHERE sqcandidato = @sqcandidato";
                return await connection.QueryFirstOrDefaultAsync<Guid>(query, new { sqcandidato });
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
