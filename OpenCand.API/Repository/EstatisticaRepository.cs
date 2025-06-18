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
                        WITH patrimonio_anual AS (
                            SELECT idcandidato, ano, SUM(valor) AS valor_total_ano
                            FROM mv_bem_candidato
                            GROUP BY idcandidato, ano
                        ),
                        extremos_declaracao AS (
                            SELECT idcandidato, MIN(ano) AS anoInicial, MAX(ano) AS anoFinal
                            FROM patrimonio_anual
                            GROUP BY idcandidato
                            HAVING COUNT(DISTINCT ano) >= 2
                        )
                        SELECT
                            c.nome,
                            ed.anoInicial,
                            pi.valor_total_ano AS patrimonioInicial,
                            ed.anoFinal,
                            pf.valor_total_ano AS patrimonioFinal,
                            (pf.valor_total_ano - pi.valor_total_ano) AS enriquecimento
                        FROM extremos_declaracao ed
                        JOIN candidato c ON ed.idcandidato = c.idcandidato
                        JOIN patrimonio_anual pi ON ed.idcandidato = pi.idcandidato AND ed.anoInicial = pi.ano
                        JOIN patrimonio_anual pf ON ed.idcandidato = pf.idcandidato AND ed.anoFinal = pf.ano
                        ORDER BY enriquecimento DESC
                        LIMIT 25;")
                    ).AsList();
                }
            });            return result ?? new List<MaioresEnriquecimento>();
        }

        public async Task<ConfigurationModel> GetConfiguration()
        {
            string cacheKey = GenerateCacheKey("GetConfigurationModel");

            var result = await GetOrSetCacheAsync(cacheKey, async () =>
            {
                var result = new ConfigurationModel();

                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    result.Partidos = (await connection.QueryAsync<string>(@"SELECT DISTINCT sigla FROM partido;")).AsList();
                    result.SiglasUF = (await connection.QueryAsync<string>(@"SELECT DISTINCT siglauf FROM candidato_mapping;")).AsList();
                    result.Anos = (await connection.QueryAsync<int>(@"SELECT DISTINCT ano FROM candidato_mapping;")).AsList();
                    result.Cargos = (await connection.QueryAsync<string>(@"SELECT DISTINCT cargo FROM candidato_mapping;")).AsList();
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
