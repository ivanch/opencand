using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenCand.Config;
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

        private readonly string BasePath;

        public ParserManager(
            IOptions<CsvSettings> csvSettings,
            ILogger<ParserManager> logger,
            IConfiguration configuration,
            CsvParserService<CandidatoCSV> candidatoParserService,
            CsvParserService<BemCandidatoCSV> bemCandidatoParserService,
            CsvParserService<RedeSocialCSV> redeSocialParserService,
            CsvParserService<DespesasCSV> despesaParserService,
            CsvParserService<ReceitasCSV> receitaParserService)
        {
            this.logger = logger;
            this.csvSettings = csvSettings.Value;

            this.candidatoParserService = candidatoParserService;
            this.bemCandidatoParserService = bemCandidatoParserService;
            this.redeSocialParserService = redeSocialParserService;
            this.despesaParserService = despesaParserService;
            this.receitaParserService = receitaParserService;

            // Get the base path from either SampleFolder in csvSettings or the BasePath in configuration
            BasePath = configuration.GetValue<string>("BasePath") ?? string.Empty;
            if (string.IsNullOrEmpty(BasePath))
            {
                throw new Exception("ParseFullDataAsync - BasePath is not configured in appsettings.json or CsvSettings.SampleFolder");
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

            //await ParseFolder(candidatosDirectory, candidatoParserService);
            //await ParseFolder(bensCandidatosDirectory, bemCandidatoParserService);
            //await ParseFolder(redesSociaisDirectory, redeSocialParserService);
            await ParseFolder(despesasDirectory, despesaParserService);
            await ParseFolder(receitasDirectory, receitaParserService);

            logger.LogInformation("ParseFullDataAsync - Full data parsing completed!");
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
