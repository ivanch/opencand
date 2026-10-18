using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using OpenCand.API.Model;
using OpenCand.Core.Models;

namespace OpenCand.Repository
{
    public class DespesaReceitaRepository : BaseRepository
    {
        // Old (merged) candidate ids keep working: they resolve to the canonical id through candidato_alias.
        private const string ResolvedId = "COALESCE((SELECT new_id FROM candidato_alias WHERE old_id = @idcandidato), @idcandidato)";

        public DespesaReceitaRepository(IConfiguration configuration, IMemoryCache? cache = null) : base(configuration, cache)
        {
        }

        public Task<(List<Despesa> Items, long Total)> GetDespesasAsync(Guid idcandidato, int? ano, Paging paging)
            => GetPageAsync<Despesa>("despesas_candidato", idcandidato, ano, paging);

        public Task<(List<Receita> Items, long Total)> GetReceitasAsync(Guid idcandidato, int? ano, Paging paging)
            => GetPageAsync<Receita>("receitas_candidato", idcandidato, ano, paging);

        public Task<List<AnoResumo>> GetDespesasResumoAsync(Guid idcandidato)
            => GetResumoAsync("despesas_candidato", idcandidato);

        public Task<List<AnoResumo>> GetReceitasResumoAsync(Guid idcandidato)
            => GetResumoAsync("receitas_candidato", idcandidato);

        // `table` is only ever one of the two constants above, never user input.
        private async Task<(List<T> Items, long Total)> GetPageAsync<T>(string table, Guid idcandidato, int? ano, Paging paging)
        {
            var where = $"WHERE idcandidato = {ResolvedId}" + (ano.HasValue ? " AND ano = @ano" : "");
            var idColumn = table == "despesas_candidato" ? "iddespesa" : "idreceita";
            var limit = paging.Enabled ? "OFFSET @offset LIMIT @limit" : "";

            using var connection = new NpgsqlConnection(ConnectionString);
            using var grid = await connection.QueryMultipleAsync($@"
                SELECT COUNT(*) FROM {table} {where};
                SELECT * FROM {table} {where}
                ORDER BY ano DESC, valor DESC, {idColumn}
                {limit};", new
            {
                idcandidato,
                ano,
                offset = paging.Offset,
                limit = paging.PageSize
            });

            var total = await grid.ReadSingleAsync<long>();
            var items = (await grid.ReadAsync<T>()).AsList();
            return (items, total);
        }

        private async Task<List<AnoResumo>> GetResumoAsync(string table, Guid idcandidato)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            return (await connection.QueryAsync<AnoResumo>($@"
                SELECT ano, COUNT(*) AS quantidade, COALESCE(SUM(valor), 0) AS total
                FROM {table}
                WHERE idcandidato = {ResolvedId}
                GROUP BY ano
                ORDER BY ano DESC;", new { idcandidato })).AsList();
        }
    }
}
