using Microsoft.Extensions.Configuration;
using Npgsql;

namespace OpenCand.Repository
{
    public abstract class BaseRepository
    {
        protected string ConnectionString { get; private set; }
        protected NpgsqlConnection? Connection { get; private set; }

        public BaseRepository(IConfiguration configuration)
        {
            ConnectionString = configuration["DatabaseSettings:ConnectionString"] ?? 
                throw new ArgumentNullException("Connection string not found in configuration");
        }
    }
}
