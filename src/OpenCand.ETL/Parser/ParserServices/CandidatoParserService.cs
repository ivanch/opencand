using System.Globalization;
using Microsoft.Extensions.Logging;
using OpenCand.Core.Models;
using OpenCand.ETL.Contracts;
using OpenCand.ETL.Extensions;
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
            if (record.CPFCandidato?.Length <= 3 || record.CPFCandidato.IsNullOrEmpty())
            {
                record.CPFCandidato = null; // Handle null/empty/whitespace CPF
            }
            else
            {
                record.CPFCandidato = record.CPFCandidato.Trim();
            }

            if (record.Apelido.IsNullOrEmpty())
            {
                record.Apelido = null;
            }

            if (record.NomeCandidato.IsNullOrEmpty())
            {
                logger.LogCritical($"ParseCandidatosAsync - Candidate with id {record.SequencialCandidato} with invalid name, skipping...");
                return; // Skip candidates with invalid name
            }

            if (record.Apelido.IsNullOrEmpty())
            {
                record.Apelido = null;
            }

            if (record.SituacaoTurno.IsNullOrEmpty())
            {
                record.SituacaoTurno = null;
            }

            var candidato = new Candidato
            {
                Cpf = record.CPFCandidato,
                SqCandidato = record.SequencialCandidato,
                Nome = record.NomeCandidato.Trim(),
                Apelido = record.Apelido?.Trim(),
                Sexo = record.Genero.Trim(),
                Localidade = record.NomeUE.Trim(),
                Ultimoano = record.AnoEleicao,
                Eleicoes = new List<CandidatoMapping>()
                {
                    new CandidatoMapping
                    {
                        Cpf = record.CPFCandidato,
                        Nome = record.NomeCandidato.Trim(),
                        SqCandidato = record.SequencialCandidato.Trim(),
                        Ano = record.AnoEleicao,
                        Turno = record.Turno.Trim(),
                        TipoEleicao = record.TipoAbrangencia.Trim(),
                        NomeUE = record.NomeUE.Trim(),
                        SiglaUF = record.SiglaUF.Trim(),
                        Cargo = record.DescricaoCargo.Trim(),
                        NrCandidato = record.NumeroCandidato.Trim(),
                        Resultado = record.SituacaoTurno?.Trim() ?? "-",
                        Partido = new Partido
                        {
                            Sigla = record.SiglaPartido.Trim(),
                            Nome = record.NomePartido.Trim(),
                            Numero = record.NumeroPartido,
                        }
                    }
                },
                CandidatoExt = new List<CandidatoExt>()
                {
                    new CandidatoExt
                    {
                        Apelido = record.Apelido?.Trim(),
                        EstadoCivil = record.EstadoCivil.Trim(),
                        Escolaridade = record.GrauInstrucao.Trim(),
                        Ocupacao = record.Ocupacao.Trim(),
                        Ano = record.AnoEleicao,
                        Email = record.Email.IsNullOrEmpty() ? null : record.Email.Trim()
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
