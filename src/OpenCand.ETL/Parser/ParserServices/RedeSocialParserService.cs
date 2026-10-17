using Microsoft.Extensions.Logging;
using OpenCand.Core.Models;
using OpenCand.ETL.Contracts;
using OpenCand.Parser.Models;
using OpenCand.Parser.Services;
using OpenCand.Services;

namespace OpenCand.ETL.Parser.ParserServices
{
    public class RedeSocialParserService : IParserService<RedeSocialCSV>
    {
        private readonly ILogger<RedeSocialParserService> logger;
        private readonly RedeSocialService redeSocialService;

        public RedeSocialParserService(
            ILogger<RedeSocialParserService> logger,
            RedeSocialService redeSocialService)
        {
            this.logger = logger;
            this.redeSocialService = redeSocialService;
        }

        public async Task ParseObject(RedeSocialCSV record)
        {
            var redeSocial = new RedeSocial
            {
                SqCandidato = record.SequencialCandidato,
                Ano = record.DataEleicao,
                SiglaUF = record.SiglaUF,
                Link = record.Url,
                Rede = string.Empty
            };

            await redeSocialService.AddRedeSocialAsync(redeSocial);
        }
    }
}
