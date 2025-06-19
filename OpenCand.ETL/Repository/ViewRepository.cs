using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OpenCand.Core.Models;
using OpenCand.Repository;

namespace OpenCand.ETL.Repository
{
    public class ViewRepository : BaseRepository
    {
        public ViewRepository(IConfiguration configuration) : base(configuration)
        { }

        public async Task RefreshMaterializedViews()
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                // Get all materialized view names
                var materializedViews = await connection.QueryAsync<string>(
                    @"SELECT schemaname || '.' || matviewname as full_name 
                      FROM pg_matviews 
                      ORDER BY schemaname, matviewname");

                foreach (var viewName in materializedViews)
                {
                    // Refresh the materialized view
                    await connection.ExecuteAsync($"REFRESH MATERIALIZED VIEW {viewName}");

                    // Analyze the materialized view to update statistics
                    await connection.ExecuteAsync($"ANALYZE {viewName}");
                }
            }
        }
    }
}
