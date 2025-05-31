using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using OpenCand.Core.Models;
using OpenCand.Repository;

namespace OpenCand.ETL.Repository
{
    public class PartidoRepository : BaseRepository
    {
        // Memory cache for partido data
        private readonly MemoryCache partidoCache = new MemoryCache(new MemoryCacheOptions());

        public PartidoRepository(IConfiguration configuration) : base(configuration)
        {
            // Initialize the cache with a sliding expiration of 5 minutes
            partidoCache = new MemoryCache(new MemoryCacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMinutes(5)
            });
        }

        public async Task AddPartidoAsync(Partido partido)
        {
            // Check if partido is already cached
            if (partidoCache.TryGetValue(partido.Sigla, out Partido? cachedPartido))
            {
                // If partido is already cached, no need to insert again
                return;
            }

            using (var connection = new Npgsql.NpgsqlConnection(ConnectionString))
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO partido (sigla, nome, numero)
                    VALUES (@sigla, @nome, @numero)
                    ON CONFLICT DO NOTHING;",
                    new
                    {
                        sigla = partido.Sigla,
                        nome = partido.Nome,
                        numero = partido.Numero
                    });

                partidoCache.Set(partido.Sigla, partido, TimeSpan.FromMinutes(5));
            }
        }
    }
}
