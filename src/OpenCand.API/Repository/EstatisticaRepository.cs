using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using OpenCand.API.Model;
using OpenCand.Repository;
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

        public async Task<List<MaioresEnriquecimento>> GetMaioresEnriquecimentos(string cacheKey, GetValueSumRequestFilter? requestFilter = null)
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
                : $"WHERE EXISTS (SELECT 1 FROM candidato_mapping cm WHERE cm.idcandidato = e.idcandidato AND {string.Join(" AND ", conditions)})";

            var result = await GetOrSetCacheAsync(cacheKey, async () =>
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    // mv_enriquecimento_candidato is indexed on enriquecimento DESC: the top 25 are read straight off the index,
                    // checking the mapping filter row by row, and candidato is joined only for those 25.
                    return (await connection.QueryAsync<MaioresEnriquecimento>($@"
                        SELECT
                            e.idcandidato,
                            c.nome,
                            e.anoinicial,
                            e.patrimonioinicial,
                            e.anofinal,
                            e.patrimoniofinal,
                            e.enriquecimento
                        FROM (
                            SELECT e.*
                            FROM mv_enriquecimento_candidato e
                            {mappingFilter}
                            ORDER BY e.enriquecimento DESC
                            LIMIT 25
                        ) e
                        JOIN candidato c ON c.idcandidato = e.idcandidato
                        ORDER BY e.enriquecimento DESC;", parameters)
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
                    // One round trip, and a single scan of candidato_mapping for the three mapping dimensions
                    // (sorted by the database, so the order matches its collation as before).
                    using var multi = await connection.QueryMultipleAsync(@"
                        SELECT DISTINCT sigla FROM partido ORDER BY sigla ASC;
                        SELECT
                            array_agg(DISTINCT siglauf ORDER BY siglauf ASC) AS siglasuf,
                            array_agg(DISTINCT ano ORDER BY ano DESC) AS anos,
                            array_agg(DISTINCT cargo ORDER BY cargo ASC) AS cargos
                        FROM (SELECT DISTINCT ano, siglauf, cargo FROM candidato_mapping) d;");

                    result.Partidos = (await multi.ReadAsync<string>()).AsList();
                    var dimensions = await multi.ReadSingleAsync<(string[]? SiglasUF, int[]? Anos, string[]? Cargos)>();

                    result.SiglasUF = dimensions.SiglasUF?.ToList() ?? new List<string>();
                    result.Anos = dimensions.Anos?.ToList() ?? new List<int>();
                    result.Cargos = dimensions.Cargos?.ToList() ?? new List<string>();
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
