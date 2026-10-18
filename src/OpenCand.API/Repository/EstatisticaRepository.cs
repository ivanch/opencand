using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using OpenCand.API.Model;
using OpenCand.Repository;
using System.Text.Json;
using static OpenCand.API.Model.GetValueSumRequest;

namespace OpenCand.API.Repository
{
    public class EstatisticaRepository : BaseRepository
    {
        private readonly IConfiguration configuration;

        public EstatisticaRepository(IConfiguration configuration, IMemoryCache? cache = null) : base(configuration, cache)
        {
            this.configuration = configuration;
        }

        public async Task<List<MaioresEnriquecimento>> GetMaioresEnriquecimentos(GetValueSumRequestFilter? requestFilter = null)
        {
            requestFilter ??= new GetValueSumRequestFilter();

            var parameters = new DynamicParameters();
            var conditions = new List<string>();
            if (!string.IsNullOrWhiteSpace(requestFilter.Partido))
            {
                conditions.Add("cm.sgpartido = @partido");
                parameters.Add("partido", requestFilter.Partido.Trim());
            }
            if (!string.IsNullOrWhiteSpace(requestFilter.Uf))
            {
                conditions.Add("cm.siglauf = @uf");
                parameters.Add("uf", requestFilter.Uf.Trim().ToUpperInvariant());
            }
            if (requestFilter.Ano.HasValue)
            {
                conditions.Add("cm.ano = @ano");
                parameters.Add("ano", requestFilter.Ano.Value);
            }
            if (!string.IsNullOrWhiteSpace(requestFilter.Cargo))
            {
                conditions.Add("cm.cargo = @cargo");
                parameters.Add("cargo", requestFilter.Cargo.Trim());
            }

            var mappingFilter = conditions.Count == 0
                ? string.Empty
                : $"WHERE EXISTS (SELECT 1 FROM candidato_mapping cm WHERE cm.idcandidato = ed.idcandidato AND {string.Join(" AND ", conditions)})";

            string cacheKey = GenerateCacheKey("GetMaioresEnriquecimento", JsonSerializer.Serialize(requestFilter));

            var result = await GetOrSetCacheAsync(cacheKey, async () =>
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    return (await connection.QueryAsync<MaioresEnriquecimento>($@"
                        WITH patrimonio AS (
                            SELECT idcandidato, ano, valor
                            FROM mv_bem_candidato_ano
                        ),
                        extremos_declaracao AS (
                            SELECT
                                idcandidato,
                                MIN(ano) AS anoInicial,
                                MAX(ano) AS anoFinal
                            FROM patrimonio
                            GROUP BY idcandidato
                            HAVING COUNT(DISTINCT ano) >= 2
                        )
                        SELECT
                            ed.idcandidato,
                            c.nome,
                            ed.anoInicial,
                            p_ini.valor AS patrimonioInicial,
                            ed.anoFinal,
                            p_fim.valor AS patrimonioFinal,
                            (p_fim.valor - p_ini.valor) AS enriquecimento
                        FROM extremos_declaracao ed
                        JOIN candidato c ON ed.idcandidato = c.idcandidato
                        JOIN patrimonio p_ini ON ed.idcandidato = p_ini.idcandidato AND ed.anoInicial = p_ini.ano
                        JOIN patrimonio p_fim ON ed.idcandidato = p_fim.idcandidato AND ed.anoFinal = p_fim.ano
                        {mappingFilter}
                        ORDER BY enriquecimento DESC
                        LIMIT 25;", parameters)
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
                    result.Anos = (await connection.QueryAsync<int>(@"SELECT DISTINCT ano FROM candidato_mapping ORDER BY ano DESC;")).AsList();
                    result.Cargos = (await connection.QueryAsync<string>(@"SELECT DISTINCT cargo FROM candidato_mapping ORDER BY cargo ASC;")).AsList();
                }

                return result;
            });

            return result ?? new ConfigurationModel();
        }

        public async Task<List<GetValueSumResponse>> GetValueSum(string cacheKey, string query, Dictionary<string, object>? parameters = null)
        {
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
