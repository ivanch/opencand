using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using OpenCand.Core.Models;
using OpenCand.ETL.Parser.CsvMappers;
using OpenCand.Parser.CsvMappers;
using OpenCand.Parser.Models;
using OpenCand.Services;

namespace OpenCand.Parser.Services
{
    public class CsvParserService
    {
        private readonly ILogger<CsvParserService> logger;
        private readonly CandidatoService candidatoService;
        private readonly BemCandidatoService bemCandidatoService;
        private readonly RedeSocialService redeSocialService;
        private readonly CsvFixerService csvFixerService;

        private readonly CsvConfiguration parserConfig;

        public CsvParserService(
            ILogger<CsvParserService> logger,
            CandidatoService candidatoService,
            BemCandidatoService bemCandidatoService,
            RedeSocialService redeSocialService,
            CsvFixerService csvFixerService)
        {
            this.logger = logger;
            this.candidatoService = candidatoService;
            this.bemCandidatoService = bemCandidatoService;
            this.redeSocialService = redeSocialService;
            this.csvFixerService = csvFixerService;

            parserConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                PrepareHeaderForMatch = args => args.Header.ToLower(),
                MissingFieldFound = null,
                TrimOptions = TrimOptions.Trim,
                Encoding = System.Text.Encoding.UTF8
            };
        }

        public async Task ParseCandidatosAsync(string filePath)
        {
            logger.LogInformation($"ParseCandidatosAsync - Starting to parse 'candidatos' from '{filePath}'");

            filePath = csvFixerService.FixCsvFile(filePath);

            // Fix the CSV file if necessary
            if (string.IsNullOrEmpty(filePath))
            {
                logger.LogError($"ParseCandidatosAsync - Failed to fix CSV file at '{filePath}'");
                throw new InvalidOperationException($"Failed to fix CSV file at '{filePath}'");
            }

            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, parserConfig);
                var po = new ParallelOptions
                {
                    MaxDegreeOfParallelism = 100
                };

                csv.Context.RegisterClassMap<CandidatoMap>();

                var records = csv.GetRecords<CandidatoCSV>();

                await Parallel.ForEachAsync(records, po, async (record, ct) =>
                {
                    try
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

                        var candidato = new Candidato
                        {
                            Cpf = record.CPFCandidato,
                            SqCandidato = record.SequencialCandidato,
                            Nome = record.NomeCandidato,
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
                                    SqCandidato = record.SequencialCandidato,
                                    Ano = record.AnoEleicao,
                                    TipoEleicao = record.TipoAbrangencia,
                                    NomeUE = record.NomeUE,
                                    SiglaUF = record.SiglaUF,
                                    Cargo = record.DescricaoCargo,
                                    NrCandidato = record.NumeroCandidato,
                                    Resultado = record.SituacaoTurno,
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
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "ParseCandidatosAsync - Error processing candidate with id {CandidatoId}", record.SequencialCandidato);
                    }
                });
                
                logger.LogInformation("ParseCandidatosAsync - Finished parsing candidatos from {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ParseCandidatosAsync - Error parsing candidatos file {FilePath}", filePath);
                throw;
            }
        }
        
        public async Task ParseBensCandidatosAsync(string filePath)
        {
            logger.LogInformation($"ParseBensCandidatosAsync - Starting to parse bens candidatos from '{filePath}'");

            filePath = csvFixerService.FixCsvFile(filePath);

            // Fix the CSV file if necessary
            if (string.IsNullOrEmpty(filePath))
            {
                logger.LogError($"ParseBensCandidatosAsync - Failed to fix CSV file at '{filePath}'");
                throw new InvalidOperationException($"Failed to fix CSV file at '{filePath}'");
            }

            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, parserConfig);
                csv.Context.RegisterClassMap<BemCandidatoMap>();

                var records = csv.GetRecords<BemCandidatoCSV>();
                
                foreach (var record in records)
                {
                    try
                    {
                        // Parse decimal value
                        decimal? valor = null;
                        if (!string.IsNullOrEmpty(record.ValorBemCandidato))
                        {
                            string normalizedValue = record.ValorBemCandidato.Replace(".", "").Replace(",", ".");
                            if (decimal.TryParse(normalizedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedValue))
                            {
                                valor = parsedValue;
                            }
                        }

                        var bemCandidato = new BemCandidato
                        {
                            SqCandidato = record.SequencialCandidato,
                            Ano = record.AnoEleicao,
                            SiglaUF = record.SiglaUF,
                            NomeUE = record.NomeUE,
                            OrdemBem = record.NumeroOrdemBemCandidato,
                            TipoBem = record.DescricaoTipoBemCandidato,
                            Descricao = record.DescricaoBemCandidato,
                            Valor = valor
                        };
                        
                        await bemCandidatoService.AddBemCandidatoAsync(bemCandidato);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "ParseBensCandidatosAsync - Error processing bem candidato with id {CandidatoId} and ordem {OrdemBem}", 
                            record.SequencialCandidato, record.NumeroOrdemBemCandidato);
                    }
                }
                
                logger.LogInformation("ParseBensCandidatosAsync - Finished parsing bens candidatos from {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ParseBensCandidatosAsync - Error parsing bens candidatos file {FilePath}", filePath);
                throw;
            }
        }

        public async Task ParseRedeSocialAsync(string filePath)
        {
            logger.LogInformation($"ParseRedeSocialAsync - Starting to parse redes sociais from '{filePath}'");

            filePath = csvFixerService.FixCsvFile(filePath);

            // Fix the CSV file if necessary
            if (string.IsNullOrEmpty(filePath))
            {
                logger.LogError($"ParseRedeSocialAsync - Failed to fix CSV file at '{filePath}'");
                throw new InvalidOperationException($"Failed to fix CSV file at '{filePath}'");
            }

            try
            {
                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, parserConfig);
                csv.Context.RegisterClassMap<RedeSocialMap>();

                var records = csv.GetRecords<RedeSocialCSV>();

                foreach (var record in records)
                {
                    try
                    {
                        var redeSocial = new RedeSocial
                        {
                            SqCandidato = record.SequencialCandidato,
                            Ano = record.DataEleicao,
                            SiglaUF = record.SiglaUF,
                            Link = record.Url,
                            Rede = string.Empty
                        };

                        await redeSocialService.AddRedeSocialAsync(redeSocial);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "ParseRedeSocialAsync - Error processing redes sociais with id {SequencialCandidato} and link {Url}",
                            record.SequencialCandidato, record.Url);
                    }
                }

                logger.LogInformation("ParseRedeSocialAsync - Finished parsing redes sociais from {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ParseRedeSocialAsync - Error parsing redes sociais file {FilePath}", filePath);
                throw;
            }
        }
    }
}
