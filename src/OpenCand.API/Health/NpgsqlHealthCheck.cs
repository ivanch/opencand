using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace OpenCand.API.Health
{
    public class NpgsqlHealthCheck : IHealthCheck
    {
        private readonly string connectionString;

        public NpgsqlHealthCheck(IConfiguration configuration)
        {
            connectionString = configuration["DatabaseSettings:ConnectionString"]
                ?? throw new InvalidOperationException("Connection string not found in configuration");
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync(cancellationToken);
                await using var command = new NpgsqlCommand("SELECT 1", connection);
                await command.ExecuteScalarAsync(cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("PostgreSQL is not reachable", ex);
            }
        }
    }
}
