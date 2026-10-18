using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using OpenCand.Config;
using OpenCand.Core.Database;
using OpenCand.ETL.Database;
using OpenCand.ETL.Loading;
using OpenCand.ETL.Pipeline;

namespace OpenCand
{
    public class Program
    {
        /// <summary>
        /// Usage: OpenCand.ETL [--only candidatos,bens,redes,despesas,receitas] [--Etl:RebuildViews=true]
        /// Exit code: 0 success, 1 failure, 2 invalid arguments.
        /// </summary>
        static async Task<int> Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using var host = CreateHostBuilder(args).Build();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                if (host.Services.GetRequiredService<IConfiguration>().GetValue("Database:AutoMigrate", true))
                    await DbMigrator.MigrateAsync(host.Services.GetRequiredService<NpgsqlDataSource>(), logger, cts.Token);

                return await host.Services.GetRequiredService<EtlRunner>().RunAsync(cts.Token);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "ETL aborted");
                return 1;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var basePath = File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))
                        ? Directory.GetCurrentDirectory()
                        : AppContext.BaseDirectory;
                    config.SetBasePath(basePath);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
                    config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true);
                    config.AddEnvironmentVariables();
                    config.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<CsvSettings>(hostContext.Configuration.GetSection("CsvSettings"));
                    services.Configure<EtlSettings>(hostContext.Configuration.GetSection("Etl"));

                    var connectionString = hostContext.Configuration["DatabaseSettings:ConnectionString"]
                        ?? throw new InvalidOperationException("DatabaseSettings:ConnectionString is not configured.");
                    services.AddSingleton(NpgsqlDataSource.Create(connectionString));

                    services.AddSingleton<SqIndex>();
                    services.AddSingleton<MaterializedViews>();
                    services.AddSingleton<EtlRunLog>();
                    services.AddSingleton<CandidatoLoader>();
                    services.AddSingleton<BemCandidatoLoader>();
                    services.AddSingleton<RedeSocialLoader>();
                    services.AddSingleton<DespesaLoader>();
                    services.AddSingleton<ReceitaLoader>();
                    services.AddSingleton<EtlRunner>();
                });
    }
}
