using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OpenCand.Core.Models;

namespace OpenCand.Repository
{
    public class RedeSocialRepository : BaseRepository
    {
        public RedeSocialRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task AddRedeSocialAsync(RedeSocial redeSocial)
        {
            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO rede_social (idcandidato, rede, siglauf, ano, link)
                    VALUES (@idcandidato, @rede, @siglauf, @ano, @link)
                    ON CONFLICT (idcandidato, rede, siglauf, ano) DO UPDATE SET
                        link = EXCLUDED.link;",
                new
                {
                    idcandidato = redeSocial.IdCandidato,
                    rede = redeSocial.Rede,
                    siglauf = redeSocial.SiglaUF,
                    ano = redeSocial.Ano,
                    link = redeSocial.Link
                });
            }
        }
    }
}
