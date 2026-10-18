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

            return await estatisticaRepository.GetMaioresEnriquecimentos(requestFilter);
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
            var f = request.Filter;
            return string.Join("|", "ValueSum", request.Type.ToLowerInvariant(), request.GroupBy.ToLowerInvariant(),
                f?.Partido?.Trim() ?? "", f?.Uf?.Trim().ToUpperInvariant() ?? "", f?.Ano?.ToString() ?? "", f?.Cargo?.Trim() ?? "");
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
                var selectClause = BuildSelectClause(request.GroupBy);
                var fromClause = BuildFromClause(request.Type);
                var joinClause = BuildJoinClause(request.GroupBy);
                var whereClause = BuildWhereClause(request.Filter);
                var groupByClause = BuildGroupByClause(request.GroupBy);
                var orderByClause = "ORDER BY SUM(src.valor) DESC";
                var limitClause = "LIMIT 10";

                return string.Join(" ", new[] { selectClause, fromClause, joinClause, whereClause, groupByClause, orderByClause, limitClause }
                    .Where(part => part.Length > 0));
            }

            private string BuildSelectClause(string groupBy)
            {
                return groupBy.ToLower() switch
                {
                    "candidato" => "SELECT src.idcandidato, c.nome, src.ano, SUM(src.valor) as valor",
                    "partido" => "SELECT src.sgpartido, src.ano, SUM(src.valor) as valor",
                    "uf" => "SELECT src.siglauf, src.ano, SUM(src.valor) as valor",
                    "cargo" => "SELECT src.cargo, src.ano, SUM(src.valor) as valor",
                    _ => throw new ArgumentException("Invalid group by specified.")
                };
            }

            private string BuildFromClause(string type)
            {
                return type.ToLower() switch
                {
                    "bem" => "FROM mv_bem_candidato src",
                    "despesa" => "FROM mv_despesas_candidato src",
                    "receita" => "FROM mv_receitas_candidato src",
                    _ => throw new ArgumentException("Invalid type specified.")
                };
            }

            // The MVs already carry siglauf / sgpartido / cargo, so only grouping by candidato needs a join (for the name).
            private string BuildJoinClause(string groupBy)
            {
                return groupBy.ToLower() switch
                {
                    "candidato" => "JOIN candidato c ON c.idcandidato = src.idcandidato",
                    "partido" or "uf" or "cargo" => "",
                    _ => throw new ArgumentException("Invalid group by specified.")
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

            private string BuildGroupByClause(string groupBy)
            {
                return groupBy.ToLower() switch
                {
                    "candidato" => "GROUP BY src.idcandidato, c.nome, src.ano",
                    "partido" => "GROUP BY src.sgpartido, src.ano",
                    "uf" => "GROUP BY src.siglauf, src.ano",
                    "cargo" => "GROUP BY src.cargo, src.ano",
                    _ => throw new ArgumentException("Invalid group by specified.")
                };
            }
        }

    }
}
