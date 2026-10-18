using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using OpenCand.Core.Models;

namespace OpenCand.Repository
{
    public class BemCandidatoRepository : BaseRepository
    {
        public BemCandidatoRepository(IConfiguration configuration, IMemoryCache? cache = null) : base(configuration, cache)
        {
        }

        public async Task<List<BemCandidato>?> GetBemCandidatoAsync(Guid idcandidato)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                // Old (merged) candidate ids resolve to the canonical id through candidato_alias.
                var query = @"
                    SELECT * FROM bem_candidato
                    WHERE idcandidato = COALESCE((SELECT new_id FROM candidato_alias WHERE old_id = @idcandidato), @idcandidato)
                    ORDER BY ano DESC, ordembem ASC;";

                return (await connection.QueryAsync<BemCandidato>(query, new { idcandidato })).AsList();
            }
        }
    }
}
