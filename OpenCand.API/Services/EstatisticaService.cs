using OpenCand.API.Model;
using OpenCand.API.Repository;
using static OpenCand.API.Model.GetValueSumRequest;

namespace OpenCand.API.Services
{
    public class EstatisticaService
    {
        private readonly EstatisticaRepository estatisticaRepository;
        private readonly ILogger<OpenCandService> logger;

        public EstatisticaService(
            EstatisticaRepository estatisticaRepository,
            ILogger<OpenCandService> logger)
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

            var result = await estatisticaRepository.GetValueSum(query, parameters);

            stopwatch.Stop();
            logger.LogInformation($"GetValueSum - Execution time: {stopwatch.ElapsedMilliseconds} ms");

            return result;
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

        private class SqlQueryBuilder
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

                return $"{selectClause} {fromClause} {joinClause} {whereClause} {groupByClause} {orderByClause} {limitClause}";
            }

            private string BuildSelectClause(string groupBy)
            {
                return groupBy.ToLower() switch
                {
                    "candidato" => "SELECT src.idcandidato, cm.nome, src.ano, SUM(src.valor) as valor",
                    "partido" => "SELECT cm.sgpartido, src.ano, SUM(src.valor) as valor",
                    "uf" => "SELECT cm.siglauf, src.ano, SUM(src.valor) as valor",
                    "cargo" => "SELECT cm.cargo, src.ano, SUM(src.valor) as valor",
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

            private string BuildJoinClause(string groupBy)
            {
                return groupBy.ToLower() switch
                {
                    "candidato" => "JOIN mv_candidato_mapping_analytics cm ON src.idcandidato = cm.idcandidato AND src.ano = cm.ano",
                    "partido" or "uf" or "cargo" => "JOIN mv_candidato_mapping_analytics cm ON src.idcandidato = cm.idcandidato AND src.ano = cm.ano",
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
                    conditions.Add($"cm.sgpartido = @{paramName}");
                    _parameters[paramName] = filter.Partido;
                }

                if (!string.IsNullOrWhiteSpace(filter.Uf))
                {
                    var paramName = $"uf{++_paramCounter}";
                    conditions.Add($"cm.siglauf = @{paramName}");
                    _parameters[paramName] = filter.Uf;
                }

                if (filter.Ano.HasValue)
                {
                    var paramName = $"ano{++_paramCounter}";
                    conditions.Add($"cm.ano = @{paramName}");
                    _parameters[paramName] = filter.Ano.Value;
                }

                if (!string.IsNullOrWhiteSpace(filter.Cargo))
                {
                    var paramName = $"cargo{++_paramCounter}";
                    conditions.Add($"cm.cargo = @{paramName}");
                    _parameters[paramName] = filter.Cargo;
                }

                return conditions.Count > 0 ? $"WHERE {string.Join(" AND ", conditions)}" : "";
            }

            private string BuildGroupByClause(string groupBy)
            {
                return groupBy.ToLower() switch
                {
                    "candidato" => "GROUP BY src.idcandidato, cm.nome, src.ano",
                    "partido" => "GROUP BY cm.sgpartido, src.ano",
                    "uf" => "GROUP BY cm.siglauf, src.ano",
                    "cargo" => "GROUP BY cm.cargo, src.ano",
                    _ => throw new ArgumentException("Invalid group by specified.")
                };
            }
        }

    }
}
