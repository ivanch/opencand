using Dapper;
using Npgsql;
using OpenCand.Core.Models;
using OpenCand.Repository;

namespace OpenCand.API.Repository
{
    public class OpenCandRepository : BaseRepository
    {
        public OpenCandRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<OpenCandStats> GetOpenCandStatsAsync()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                var stats = await connection.QueryFirstOrDefaultAsync<OpenCandStats>(@"
                    SELECT 
                        (SELECT COUNT(idcandidato) FROM candidato) AS TotalCandidatos,
                        (SELECT COUNT(*) FROM bem_candidato) AS TotalBemCandidatos,
                        (SELECT SUM(valor) FROM bem_candidato) AS TotalValorBemCandidatos,
                        (SELECT COUNT(*) FROM rede_social) AS TotalRedesSociais,
                        (SELECT COUNT(DISTINCT ano) FROM candidato_mapping) AS TotalEleicoes;");
                return stats ?? new OpenCandStats();
            }
        }
    }
}
