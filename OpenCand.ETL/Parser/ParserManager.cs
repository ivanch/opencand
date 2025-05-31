using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenCand.Config;
using OpenCand.Parser.Services;

namespace OpenCand.Parser
{
    public class ParserManager
    {
        private readonly CsvParserService csvParserService;
        private readonly ILogger<ParserManager> logger;
        private readonly CsvSettings csvSettings;
        private readonly IConfiguration configuration;

        public ParserManager(
            CsvParserService csvParserService,
            IOptions<CsvSettings> csvSettings,
            ILogger<ParserManager> logger,
            IConfiguration configuration)
        {
            this.csvParserService = csvParserService;
            this.logger = logger;
            this.csvSettings = csvSettings.Value;
            this.configuration = configuration;
        }

        public async Task ParseFullDataAsync()
        {
            logger.LogInformation("ParseFullDataAsync - Starting parsing");

            // Get the base path from either SampleFolder in csvSettings or the BasePath in configuration
            var basePath = configuration.GetValue<string>("BasePath");

            if (string.IsNullOrEmpty(basePath))
            {
                logger.LogError("ParseFullDataAsync - BasePath is not configured in appsettings.json or CsvSettings.SampleFolder");
                return;
            }

            logger.LogInformation("ParseFullDataAsync - Processing will happen with BasePath: {BasePath}", basePath);

            try
            {
                var candidatosDirectory = Path.Combine(basePath, csvSettings.CandidatosFolder);
                var bensCandidatosDirectory = Path.Combine(basePath, csvSettings.BensCandidatosFolder);
                var redesSociaisDirectory = Path.Combine(basePath, csvSettings.RedesSociaisFolder);

                if (Directory.Exists(candidatosDirectory))
                {
                    foreach (var filePath in Directory.GetFiles(candidatosDirectory, "*.csv"))
                    {
                        // Check if filePath contains "fix_" prefix
                        if (filePath.Contains("fix_"))
                        {
                            logger.LogInformation("ParseFullDataAsync - Skipping already fixed file: {FilePath}", filePath);
                            continue;
                        }

                        logger.LogInformation("ParseFullDataAsync - Parsing candidatos data from {FilePath}", filePath);
                        await csvParserService.ParseCandidatosAsync(filePath);
                    }
                }
                else
                {
                    logger.LogWarning("ParseFullDataAsync - 'Candidatos' directory not found at {Directory}", candidatosDirectory);
                }

                if (Directory.Exists(bensCandidatosDirectory))
                {
                    foreach (var filePath in Directory.GetFiles(bensCandidatosDirectory, "*.csv"))
                    {
                        // Check if filePath contains "fix_" prefix
                        if (filePath.Contains("fix_"))
                        {
                            logger.LogInformation("ParseFullDataAsync - Skipping already fixed file: {FilePath}", filePath);
                            continue;
                        }

                        logger.LogInformation("ParseFullDataAsync - Parsing bens candidatos data from {FilePath}", filePath);
                        await csvParserService.ParseBensCandidatosAsync(filePath);
                    }
                }
                else
                {
                    logger.LogWarning("ParseFullDataAsync - 'Bens candidatos' directory not found at {Directory}", bensCandidatosDirectory);
                }

                if (Directory.Exists(redesSociaisDirectory))
                {
                    foreach (var filePath in Directory.GetFiles(redesSociaisDirectory, "*.csv"))
                    {
                        // Check if filePath contains "fix_" prefix
                        if (filePath.Contains("fix_"))
                        {
                            logger.LogInformation("ParseFullDataAsync - Skipping already fixed file: {FilePath}", filePath);
                            continue;
                        }

                        logger.LogInformation("ParseFullDataAsync - Parsing redes sociais data from {FilePath}", filePath);
                        await csvParserService.ParseRedeSocialAsync(filePath);
                    }
                }
                else
                {
                    logger.LogWarning("ParseFullDataAsync - 'Redes sociais' directory not found at {Directory}", redesSociaisDirectory);
                }

                logger.LogInformation("ParseFullDataAsync - Full data parsing completed!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ParseFullDataAsync - Error parsing full data set");
                throw;
            }
        }
    }
}
