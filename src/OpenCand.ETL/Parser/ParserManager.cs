using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenCand.Config;
using OpenCand.ETL.Repository;
using OpenCand.ETL.Services;
using OpenCand.Parser.Models;
using OpenCand.Parser.Services;

namespace OpenCand.Parser
{
    public class ParserManager
    {
        private readonly ILogger<ParserManager> logger;
        private readonly CsvSettings csvSettings;

        private readonly CsvParserService<CandidatoCSV> candidatoParserService;
        private readonly CsvParserService<BemCandidatoCSV> bemCandidatoParserService;
        private readonly CsvParserService<RedeSocialCSV> redeSocialParserService;
        private readonly CsvParserService<DespesasCSV> despesaParserService;
        private readonly CsvParserService<ReceitasCSV> receitaParserService;

        private readonly DespesaReceitaService despesaReceitaService;

        private readonly ViewRepository viewRepository;

        private readonly string BasePath;

        public ParserManager(
            IOptions<CsvSettings> csvSettings,
            ILogger<ParserManager> logger,
            IConfiguration configuration,
            CsvParserService<CandidatoCSV> candidatoParserService,
            CsvParserService<BemCandidatoCSV> bemCandidatoParserService,
            CsvParserService<RedeSocialCSV> redeSocialParserService,
            CsvParserService<DespesasCSV> despesaParserService,
            CsvParserService<ReceitasCSV> receitaParserService,
            DespesaReceitaService despesaReceitaService,
            ViewRepository viewRepository)
        {
            this.logger = logger;
            this.csvSettings = csvSettings.Value;

            this.candidatoParserService = candidatoParserService;
            this.bemCandidatoParserService = bemCandidatoParserService;
            this.redeSocialParserService = redeSocialParserService;
            this.despesaParserService = despesaParserService;
            this.receitaParserService = receitaParserService;
            this.despesaReceitaService = despesaReceitaService;
            this.viewRepository = viewRepository;

            // Get the base path from either SampleFolder in csvSettings or the BasePath in configuration
            BasePath = configuration.GetValue<string>("BasePath") ?? string.Empty;
            if (string.IsNullOrEmpty(BasePath))
            {
                throw new Exception("ParseFullDataAsync - BasePath is not configured in appsettings.json or CsvSettings.SampleFolder");
            }

            if (!Directory.Exists(BasePath))
            {
                var candidatePaths = new[]
                {
                    Path.GetFullPath(BasePath),
                    Path.Combine(Directory.GetCurrentDirectory(), BasePath),
                    Path.Combine(AppContext.BaseDirectory, BasePath),
                    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../sample")),
                    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../sample")),
                    Path.Combine(Directory.GetCurrentDirectory(), "sample"),
                };

                var foundPath = candidatePaths.FirstOrDefault(p => Directory.Exists(p));
                if (foundPath != null)
                {
                    BasePath = foundPath;
                }
            }
        }

        public async Task ParseFullDataAsync()
        {
            logger.LogInformation("ParseFullDataAsync - Starting parsing");
            logger.LogInformation("ParseFullDataAsync - Processing will happen with BasePath: {BasePath}", BasePath);

            var candidatosDirectory = Path.Combine(BasePath, csvSettings.CandidatosFolder);
            var bensCandidatosDirectory = Path.Combine(BasePath, csvSettings.BensCandidatosFolder);
            var redesSociaisDirectory = Path.Combine(BasePath, csvSettings.RedesSociaisFolder);
            var despesasDirectory = Path.Combine(BasePath, csvSettings.DespesaCandidatoFolder);
            var receitasDirectory = Path.Combine(BasePath, csvSettings.ReceitaCandidatoFolder);

            await ParseFolder(candidatosDirectory, candidatoParserService);
            await ParseFolder(bensCandidatosDirectory, bemCandidatoParserService);
            await ParseFolder(redesSociaisDirectory, redeSocialParserService);

            await despesaReceitaService.DeleteDespesaAsync();
            await ParseFolder(despesasDirectory, despesaParserService);
            await despesaReceitaService.DeleteReceitaAsync();
            await ParseFolder(receitasDirectory, receitaParserService);

            logger.LogInformation("ParseFullDataAsync - Full data parsing completed!");

            logger.LogInformation("ParseFullDataAsync - Will refresh materialized views and re-run the analyzes.");
            
            await viewRepository.RefreshMaterializedViews();
            
            logger.LogInformation("ParseFullDataAsync - Materialized views refreshed successfully!");
        }

        private async Task ParseFolder<CsvObj>(string csvDirectory, CsvParserService<CsvObj> csvParserService)
        {
            if (Directory.Exists(csvDirectory))
            {
                foreach (var filePath in Directory.GetFiles(csvDirectory, "*.csv"))
                {
                    // Check if filePath contains "fix_" prefix
                    if (filePath.Contains("fix_"))
                    {
                        logger.LogInformation("ParseFolder - Skipping already fixed file: {FilePath}", filePath);
                        continue;
                    }

                    logger.LogInformation("ParseFolder - Parsing data from {FilePath}", filePath);
                    await csvParserService.ParseFolderAsync(filePath);
                }
            }
            else
            {
                logger.LogWarning("ParseFolder - Directory not found at {Directory}", csvDirectory);
            }
        }
    }
}
