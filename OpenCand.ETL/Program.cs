using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenCand.Config;
using OpenCand.ETL.Contracts;
using OpenCand.ETL.Parser.ParserServices;
using OpenCand.ETL.Repository;
using OpenCand.Parser;
using OpenCand.Parser.Models;
using OpenCand.Parser.Services;
using OpenCand.Repository;
using OpenCand.Services;

namespace OpenCand
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    logger.LogInformation("Starting data parsing");
                    var parserManager = services.GetRequiredService<ParserManager>();
                    await parserManager.ParseFullDataAsync();

                    logger.LogInformation("Data parsing completed successfully!");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred during application startup");
                }
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true);
                    config.AddEnvironmentVariables();
                    config.AddCommandLine(args);

                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Configuration
                    services.Configure<CsvSettings>(hostContext.Configuration.GetSection("CsvSettings"));

                    // Services
                    services.AddTransient<IParserService<CandidatoCSV>, CandidatoParserService>();
                    services.AddTransient<IParserService<BemCandidatoCSV>, BemCandidatoParserService>();
                    services.AddTransient<IParserService<RedeSocialCSV>, RedeSocialParserService>();
                    services.AddTransient<CsvParserService<CandidatoCSV>>();
                    services.AddTransient<CsvParserService<BemCandidatoCSV>>();
                    services.AddTransient<CsvParserService<RedeSocialCSV>>();
                    services.AddTransient<ParserManager>();
                    services.AddTransient<CandidatoService>();
                    services.AddTransient<BemCandidatoService>();
                    services.AddTransient<RedeSocialService>();
                    services.AddTransient<CandidatoRepository>();
                    services.AddTransient<BemCandidatoRepository>();
                    services.AddTransient<RedeSocialRepository>();
                    services.AddTransient<PartidoRepository>();
                    services.AddTransient<CsvFixerService>();
                });
    }
}
