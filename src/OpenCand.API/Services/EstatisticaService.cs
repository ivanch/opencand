using OpenCand.API.Model;
using OpenCand.API.Repository;
using static OpenCand.API.Model.GetValueSumRequest;

namespace OpenCand.API.Services
{
    public class EstatisticaService
    {
        private readonly EstatisticaRepository estatisticaRepository;
        private readonly ILogger<EstatisticaService> logger;

        public EstatisticaService(
            EstatisticaRepository estatisticaRepository,
            ILogger<EstatisticaService> logger)
        {
            this.estatisticaRepository = estatisticaRepository;
            this.logger = logger;
        }

        public async Task<ConfigurationModel> GetConfigurationModel()
        {
            logger.LogInformation("Getting configuration model");

            return await estatisticaRepository.GetConfiguration();
        }

        public async Task<List<MaioresEnriquecimento>> GetMaioresEnriquecimentos(GetValueSumRequestFilter? requestFilter = null)
        {
            logger.LogInformation($"Getting maiores enriquecimentos. Filters: Partido={requestFilter?.Partido}, Uf={requestFilter?.Uf}, Ano={requestFilter?.Ano}, Cargo={requestFilter?.Cargo}");

            return await estatisticaRepository.GetMaioresEnriquecimentos(BuildEnriquecimentoCacheKey(requestFilter), requestFilter);
        }

        public async Task<List<GetValueSumResponse>> GetValueSum(GetValueSumRequest request)
        {
            logger.LogInformation($"Getting value sum for {request.Type} grouped by {request.GroupBy}. Filters: Partido={request.Filter?.Partido}, Uf={request.Filter?.Uf}, Ano={request.Filter?.Ano}, Cargo={request.Filter?.Cargo}");
            // count exec time
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            ValidateRequest(request);

            var sqlBuilder = new SqlQueryBuilder();
            var query = sqlBuilder.BuildQuery(request);
            var parameters = sqlBuilder.GetParameters();

            var result = await estatisticaRepository.GetValueSum(BuildValueSumCacheKey(request), query, parameters);

            stopwatch.Stop();
            logger.LogInformation($"GetValueSum - Execution time: {stopwatch.ElapsedMilliseconds} ms");

            return result;
        }

        // Every filter value must be part of the key: the SQL text alone is identical for different filter values.
        internal static string BuildValueSumCacheKey(GetValueSumRequest request)
        {
            return string.Join("|", "ValueSum", request.Type.ToLowerInvariant(), request.GroupBy.ToLowerInvariant(), BuildFilterCacheKey(request.Filter));
        }

        internal static string BuildEnriquecimentoCacheKey(GetValueSumRequestFilter? filter)
        {
            return string.Join("|", "MaioresEnriquecimento", BuildFilterCacheKey(filter));
        }

        // Normalized the same way the filters are bound as parameters (trimmed, UF upper-cased).
        private static string BuildFilterCacheKey(GetValueSumRequestFilter? f)
        {
            return string.Join("|", f?.Partido?.Trim() ?? "", f?.Uf?.Trim().ToUpperInvariant() ?? "", f?.Ano?.ToString() ?? "", f?.Cargo?.Trim() ?? "");
        }

        private void ValidateRequest(GetValueSumRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Type))
                throw new ArgumentException("Type is required.");

            if (string.IsNullOrWhiteSpace(request.GroupBy))
                throw new ArgumentException("GroupBy is required.");

            var validTypes = new[] { "bem", "despesa", "receita" };
            if (!validTypes.Contains(request.Type.ToLower()))
                throw new ArgumentException($"Invalid type specified. Valid values are: {string.Join(", ", validTypes)}");

            var validGroupBy = new[] { "candidato", "partido", "uf", "cargo" };
            if (!validGroupBy.Contains(request.GroupBy.ToLower()))
                throw new ArgumentException($"Invalid groupBy specified. Valid values are: {string.Join(", ", validGroupBy)}");
        }

        internal class SqlQueryBuilder
        {
            private readonly Dictionary<string, object> _parameters = new();
            private int _paramCounter = 0;

            public Dictionary<string, object> GetParameters() => _parameters;

            public string BuildQuery(GetValueSumRequest request)
            {
                var groupBy = request.GroupBy.ToLower();
                var column = groupBy switch
                {
                    "candidato" => "src.idcandidato",
                    "partido" => "src.sgpartido",
                    "uf" => "src.siglauf",
                    "cargo" => "src.cargo",
                    _ => throw new ArgumentException("Invalid group by specified.")
                };
                // The candidato grouping needs the per-candidate view; the others only need the much smaller rollup.
                var view = groupBy == "candidato" ? BuildCandidatoView(request.Type) : BuildResumoView(request.Type);
                var whereClause = BuildWhereClause(request.Filter);

                var query = string.Join(" ", new[]
                {
                    $"SELECT {column}, src.ano, SUM(src.valor) AS valor",
                    $"FROM {view} src",
                    whereClause,
                    $"GROUP BY {column}, src.ano",
                    "ORDER BY SUM(src.valor) DESC",
                    "LIMIT 10"
                }.Where(part => part.Length > 0));

                // Pick the top 10 first, then join candidato only for their names.
                return groupBy == "candidato"
                    ? $"SELECT t.idcandidato, c.nome, t.ano, t.valor FROM ({query}) t JOIN candidato c ON c.idcandidato = t.idcandidato ORDER BY t.valor DESC"
                    : query;
            }

            private static string BuildCandidatoView(string type)
            {
                return type.ToLower() switch
                {
                    "bem" => "mv_bem_candidato",
                    "despesa" => "mv_despesas_candidato",
                    "receita" => "mv_receitas_candidato",
                    _ => throw new ArgumentException("Invalid type specified.")
                };
            }

            // (ano, siglauf, sgpartido, cargo) rollups of the per-candidate views
            private static string BuildResumoView(string type)
            {
                return type.ToLower() switch
                {
                    "bem" => "mv_bem_resumo",
                    "despesa" => "mv_despesas_resumo",
                    "receita" => "mv_receitas_resumo",
                    _ => throw new ArgumentException("Invalid type specified.")
                };
            }

            private string BuildWhereClause(GetValueSumRequest.GetValueSumRequestFilter? filter)
            {
                if (filter == null)
                    return "";

                var conditions = new List<string>();

                if (!string.IsNullOrWhiteSpace(filter.Partido))
                {
                    var paramName = $"partido{++_paramCounter}";
                    conditions.Add($"src.sgpartido = @{paramName}");
                    _parameters[paramName] = filter.Partido.Trim();
                }

                if (!string.IsNullOrWhiteSpace(filter.Uf))
                {
                    var paramName = $"uf{++_paramCounter}";
                    conditions.Add($"src.siglauf = @{paramName}");
                    _parameters[paramName] = filter.Uf.Trim().ToUpperInvariant();
                }

                if (filter.Ano.HasValue)
                {
                    var paramName = $"ano{++_paramCounter}";
                    conditions.Add($"src.ano = @{paramName}");
                    _parameters[paramName] = filter.Ano.Value;
                }

                if (!string.IsNullOrWhiteSpace(filter.Cargo))
                {
                    var paramName = $"cargo{++_paramCounter}";
                    conditions.Add($"src.cargo = @{paramName}");
                    _parameters[paramName] = filter.Cargo.Trim();
                }

                return conditions.Count > 0 ? $"WHERE {string.Join(" AND ", conditions)}" : "";
            }
        }

    }
}
