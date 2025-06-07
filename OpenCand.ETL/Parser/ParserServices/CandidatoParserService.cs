using System.Globalization;
using Microsoft.Extensions.Logging;
using OpenCand.Core.Models;
using OpenCand.ETL.Contracts;
using OpenCand.Parser.Models;
using OpenCand.Services;

namespace OpenCand.ETL.Parser.ParserServices
{
    public class CandidatoParserService : IParserService<CandidatoCSV>
    {
        private readonly ILogger<CandidatoParserService> logger;
        private readonly CandidatoService candidatoService;

        public CandidatoParserService(
            ILogger<CandidatoParserService> logger,
            CandidatoService candidatoService)
        {
            this.logger = logger;
            this.candidatoService = candidatoService;
        }

        public async Task ParseObject(CandidatoCSV record)
        {
            if (string.IsNullOrWhiteSpace(record.CPFCandidato) || record.CPFCandidato.Length <= 3)
            {
                record.CPFCandidato = null; // Handle null/empty/whitespace CPF
            }

            if (record.NomeCandidato == "NÃO DIVULGÁVEL" ||
            string.IsNullOrEmpty(record.NomeCandidato) ||
                record.NomeCandidato == "#NULO")
            {
                logger.LogCritical($"ParseCandidatosAsync - Candidate with id {record.SequencialCandidato} with invalid name, skipping...");
                return; // Skip candidates with invalid name
            }

            if (string.IsNullOrWhiteSpace(record.Apelido) ||
                record.Apelido.Contains("#NUL") ||
                record.Apelido.Contains("NULO#") ||
                record.Apelido.Contains("#NE"))
            {
                record.Apelido = null;
            }

            var candidato = new Candidato
            {
                Cpf = record.CPFCandidato,
                SqCandidato = record.SequencialCandidato,
                Nome = record.NomeCandidato,
                Apelido = record.Apelido,
                Email = record.Email.Contains("@") ? record.Email : null,
                Sexo = record.Genero,
                EstadoCivil = record.EstadoCivil,
                Escolaridade = record.GrauInstrucao,
                Ocupacao = record.Ocupacao,
                Eleicoes = new List<CandidatoMapping>()
                            {
                                new CandidatoMapping
                                {
                                    Cpf = record.CPFCandidato,
                                    Nome = record.NomeCandidato,
                                    Apelido = record.Apelido,
                                    SqCandidato = record.SequencialCandidato,
                                    Ano = record.AnoEleicao,
                                    Turno = record.Turno,
                                    TipoEleicao = record.TipoAbrangencia,
                                    NomeUE = record.NomeUE,
                                    SiglaUF = record.SiglaUF,
                                    Cargo = record.DescricaoCargo,
                                    NrCandidato = record.NumeroCandidato,
                                    Resultado = record.SituacaoTurno,
                                    Partido = new Partido
                                    {
                                        Sigla = record.SiglaPartido,
                                        Nome = record.NomePartido,
                                        Numero = record.NumeroPartido,
                                    }
                                }
                            }
            };

            if (!string.IsNullOrEmpty(record.DataNascimento) &&
                record.DataNascimento != "#NULO")
            {
                if (DateTime.TryParseExact(record.DataNascimento, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dataNascimento))
                {
                    // Convert to UTC DateTime to work with PostgreSQL timestamp with time zone
                    candidato.DataNascimento = DateTime.SpecifyKind(dataNascimento, DateTimeKind.Utc);
                }
            }
            else
            {
                candidato.DataNascimento = null; // Handle null/empty/whitespace date
            }

            await candidatoService.AddCandidatoAsync(candidato);
        }
    }
}
