using Microsoft.Extensions.Logging;
using OpenCand.Core.Models;
using System.Globalization;
using OpenCand.ETL.Contracts;
using OpenCand.Parser.Models;
using OpenCand.Services;
using OpenCand.Parser.Services;
using OpenCand.ETL.Extensions;

namespace OpenCand.ETL.Parser.ParserServices
{
    public class BemCandidatoParserService : IParserService<BemCandidatoCSV>
    {
        private readonly ILogger<BemCandidatoParserService> logger;
        private readonly BemCandidatoService bemCandidatoService;

        public BemCandidatoParserService(
            ILogger<BemCandidatoParserService> logger,
            BemCandidatoService bemCandidatoService)
        {
            this.logger = logger;
            this.bemCandidatoService = bemCandidatoService;
        }

        public async Task ParseObject(BemCandidatoCSV record)
        {
            // Parse decimal value
            decimal? valor = null;
            if (!record.ValorBemCandidato.IsNullOrEmpty())
            {
                string normalizedValue = record.ValorBemCandidato.Replace(".", "").Replace(",", ".");
                if (decimal.TryParse(normalizedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedValue))
                {
                    valor = parsedValue;
                }
            }

            var bemCandidato = new BemCandidato
            {
                SqCandidato = record.SequencialCandidato,
                Ano = record.AnoEleicao,
                SiglaUF = record.SiglaUF,
                NomeUE = record.NomeUE,
                OrdemBem = record.NumeroOrdemBemCandidato,
                TipoBem = record.DescricaoTipoBemCandidato,
                Descricao = record.DescricaoBemCandidato,
                Valor = valor
            };

            await bemCandidatoService.AddBemCandidatoAsync(bemCandidato);
        }
    }
}
