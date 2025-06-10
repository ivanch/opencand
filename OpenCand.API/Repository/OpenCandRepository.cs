using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using OpenCand.Core.Models;
using OpenCand.Repository;

namespace OpenCand.API.Repository
{    public class OpenCandRepository : BaseRepository
    {
        public OpenCandRepository(IConfiguration configuration, IMemoryCache? cache = null) : base(configuration, cache)
        {
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
        }

        /// <summary>
        /// Clears the cached OpenCand statistics, forcing a fresh fetch on the next call
        /// </summary>
        public void ClearStatsCache()
        {
            ClearCache(GenerateCacheKey("OpenCandStats"));
        }
    }
}
