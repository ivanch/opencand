using OpenCand.API.Model;
using OpenCand.API.Repository;
using OpenCand.Repository;

namespace OpenCand.API.Services
{
    public class EstatisticaService
    {
        private readonly OpenCandRepository openCandRepository;
        private readonly CandidatoRepository candidatoRepository;
        private readonly BemCandidatoRepository bemCandidatoRepository;
        private readonly DespesaReceitaRepository despesaReceitaRepository;
        private readonly EstatisticaRepository estatisticaRepository;
        private readonly IConfiguration configuration;
        private readonly ILogger<OpenCandService> logger;

        public EstatisticaService(
            OpenCandRepository openCandRepository,
            CandidatoRepository candidatoRepository,
            BemCandidatoRepository bemCandidatoRepository,
            DespesaReceitaRepository despesaReceitaRepository,
            EstatisticaRepository estatisticaRepository,
            IConfiguration configuration,
            ILogger<OpenCandService> logger)
        {
            this.openCandRepository = openCandRepository;
            this.candidatoRepository = candidatoRepository;
            this.bemCandidatoRepository = bemCandidatoRepository;
            this.despesaReceitaRepository = despesaReceitaRepository;
            this.estatisticaRepository = estatisticaRepository;
            this.configuration = configuration;
            this.logger = logger;
        }

        public async Task<ConfigurationModel> GetConfigurationModel()
        {
            return await estatisticaRepository.GetConfiguration();
        }

        public async Task<List<MaioresEnriquecimento>> GetMaioresEnriquecimentos()
        {
            return await estatisticaRepository.GetMaioresEnriquecimentos();
        }

        public async Task<List<GetValueSumResponse>> GetValueSum(GetValueSumRequest request)
        {
            ValidateRequest(request);

            var sqlBuilder = new SqlQueryBuilder();
            var query = sqlBuilder.BuildQuery(request);
            var parameters = sqlBuilder.GetParameters();

            return await estatisticaRepository.GetValueSum(query, parameters);
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
                    "candidato" => "SELECT src.idcandidato, c.nome, src.ano, SUM(src.valor) as valor",
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
                    "bem" => "FROM bem_candidato src",
                    "despesa" => "FROM despesas_candidato src",
                    "receita" => "FROM receitas_candidato src",
                    _ => throw new ArgumentException("Invalid type specified.")
                };
            }

            private string BuildJoinClause(string groupBy)
            {
                return groupBy.ToLower() switch
                {
                    "candidato" => "JOIN candidato c ON src.idcandidato = c.idcandidato JOIN candidato_mapping cm ON src.idcandidato = cm.idcandidato AND src.ano = cm.ano",
                    "partido" or "uf" or "cargo" => "JOIN candidato_mapping cm ON src.idcandidato = cm.idcandidato AND src.ano = cm.ano",
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
                    conditions.Add($"src.ano = @{paramName}");
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
                    "candidato" => "GROUP BY src.idcandidato, c.nome, src.ano",
                    "partido" => "GROUP BY cm.sgpartido, src.ano",
                    "uf" => "GROUP BY cm.siglauf, src.ano",
                    "cargo" => "GROUP BY cm.cargo, src.ano",
                    _ => throw new ArgumentException("Invalid group by specified.")
                };
            }
        }

    }
}
