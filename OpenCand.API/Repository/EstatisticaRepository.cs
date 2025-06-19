using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using OpenCand.API.Model;
using OpenCand.Core.Models;
using OpenCand.Repository;

namespace OpenCand.API.Repository
{
    public class EstatisticaRepository : BaseRepository
    {
        private readonly IConfiguration configuration;

        public EstatisticaRepository(IConfiguration configuration, IMemoryCache? cache = null) : base(configuration, cache)
        {
            this.configuration = configuration;
        }

        public async Task<List<MaioresEnriquecimento>> GetMaioresEnriquecimentos()
        {
            string cacheKey = GenerateCacheKey("GetMaioresEnriquecimento");

            var result = await GetOrSetCacheAsync(cacheKey, async () =>
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    return (await connection.QueryAsync<MaioresEnriquecimento>(@"
                        WITH extremos_declaracao AS (
                            SELECT
                                idcandidato,
                                MIN(ano) AS anoInicial,
                                MAX(ano) AS anoFinal
                            FROM mv_bem_candidato
                            GROUP BY idcandidato
                            HAVING COUNT(ano) >= 2
                        )
                        SELECT
                            ed.idcandidato,
                            c.nome,
                            ed.anoInicial,
                            pi.valor AS patrimonioInicial,
                            ed.anoFinal,
                            pf.valor AS patrimonioFinal, 
                            (pf.valor - pi.valor) AS enriquecimento
                        FROM extremos_declaracao ed
                        JOIN candidato c ON ed.idcandidato = c.idcandidato
                        JOIN mv_bem_candidato pi ON ed.idcandidato = pi.idcandidato AND ed.anoInicial = pi.ano
                        JOIN mv_bem_candidato pf ON ed.idcandidato = pf.idcandidato AND ed.anoFinal = pf.ano
                        ORDER BY
                            enriquecimento DESC
                        LIMIT 25;")
                    ).AsList();
                }
            });
            return result ?? new List<MaioresEnriquecimento>();
        }

        public async Task<ConfigurationModel> GetConfiguration()
        {
            string cacheKey = GenerateCacheKey("GetConfigurationModel");

            var result = await GetOrSetCacheAsync(cacheKey, async () =>
            {
                var result = new ConfigurationModel();

                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    result.Partidos = (await connection.QueryAsync<string>(@"SELECT DISTINCT sigla FROM partido ORDER BY sigla ASC;")).AsList();
                    result.SiglasUF = (await connection.QueryAsync<string>(@"SELECT DISTINCT siglauf FROM candidato_mapping ORDER BY siglauf ASC;")).AsList();
                    result.Anos = (await connection.QueryAsync<int>(@"SELECT DISTINCT ano FROM candidato_mapping ORDER BY ano ASC;")).AsList();
                    result.Cargos = (await connection.QueryAsync<string>(@"SELECT DISTINCT cargo FROM candidato_mapping ORDER BY cargo ASC;")).AsList();
                }

                return result;
            });

            return result ?? new ConfigurationModel();
        }

        public async Task<List<GetValueSumResponse>> GetValueSum(string query, Dictionary<string, object>? parameters = null)
        {
            string cacheKey = GenerateCacheKey(query.GetHashCode().ToString());
            return await GetOrSetCacheAsync(cacheKey, async () =>
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    return (await connection.QueryAsync<GetValueSumResponse>(query, parameters)).AsList();
                }
            }) ?? new List<GetValueSumResponse>();
        }
    }
}
