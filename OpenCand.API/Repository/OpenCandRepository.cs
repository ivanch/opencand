using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using OpenCand.Core.Models;
using OpenCand.Repository;

namespace OpenCand.API.Repository
{    public class OpenCandRepository : BaseRepository
    {
        private readonly IConfiguration configuration;

        public OpenCandRepository(IConfiguration configuration, IMemoryCache? cache = null) : base(configuration, cache)
        {
            this.configuration = configuration;
        }

        public async Task<OpenCandStats> GetOpenCandStatsAsync()
        {
            string cacheKey = GenerateCacheKey("OpenCandStats");
            
            var result = await GetOrSetCacheAsync(cacheKey, async () =>
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    var stats = await connection.QueryFirstOrDefaultAsync<OpenCandStats>(@"
                        SELECT 
                            (SELECT COUNT(idcandidato) FROM candidato) AS TotalCandidatos,
                            (SELECT COUNT(*) FROM bem_candidato) AS TotalBemCandidatos,
                            (SELECT SUM(valor) FROM bem_candidato) AS TotalValorBemCandidatos,
                            (SELECT COUNT(*) FROM rede_social) AS TotalRedesSociais,
                            (SELECT COUNT(DISTINCT ano) FROM candidato_mapping) AS TotalEleicoes;");
                    
                    return stats ?? new OpenCandStats();
                }
            });
            
            return result ?? new OpenCandStats();
        }        public async Task<DataAvailabilityStats> GetDataAvailabilityAsync()
        {
            string cacheKey = GenerateCacheKey("DataAvailabilityStats");

            var result = await GetOrSetCacheAsync(cacheKey, async () =>
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    var stats = new DataAvailabilityStats();

                    // Get years for each data type separately
                    var candidatosYears = await connection.QueryAsync<int>("SELECT DISTINCT ano FROM candidato_mapping ORDER BY ano DESC");
                    var bemCandidatosYears = await connection.QueryAsync<int>("SELECT DISTINCT ano FROM bem_candidato ORDER BY ano DESC");
                    var despesaCandidatosYears = await connection.QueryAsync<int>("SELECT DISTINCT ano FROM despesas_candidato ORDER BY ano DESC");
                    var receitaCandidatosYears = await connection.QueryAsync<int>("SELECT DISTINCT ano FROM receitas_candidato ORDER BY ano DESC");
                    var redeSocialCandidatosYears = await connection.QueryAsync<int>("SELECT DISTINCT ano FROM rede_social ORDER BY ano DESC");

                    stats.Candidatos = candidatosYears.ToList();
                    stats.BemCandidatos = bemCandidatosYears.ToList();
                    stats.DespesaCandidatos = despesaCandidatosYears.ToList();
                    stats.ReceitaCandidatos = receitaCandidatosYears.ToList();
                    stats.RedeSocialCandidatos = redeSocialCandidatosYears.ToList();

                    // Get all folders from appsetting `FotosSettings__BasePath`
                    string basePath = configuration["FotosSettings:Path"] ?? string.Empty;
                    if (string.IsNullOrEmpty(basePath))
                        throw new InvalidOperationException("Base path for photos is not configured.");

                    var directories = Directory.GetDirectories(basePath);
                    if (directories.Any())
                        stats.FotosCandidatos = directories
                            .Select(dir => dir.Split(Path.DirectorySeparatorChar).Last().Split("_")[1].Replace("cand", ""))
                            .Select(ano => Convert.ToInt32(ano))
                            .Distinct()
                            .OrderByDescending(ano => ano)
                            .ToList();

                    return stats;
                }
            });

            return result ?? new DataAvailabilityStats();
        }
    }
}
