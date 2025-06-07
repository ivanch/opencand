using Microsoft.Extensions.Logging;
using OpenCand.Core.Models;
using OpenCand.ETL.Contracts;
using OpenCand.ETL.Extensions;
using OpenCand.ETL.Services;
using OpenCand.Parser.Models;

namespace OpenCand.ETL.Parser.ParserServices
{
    public class ReceitaParserService : IParserService<ReceitasCSV>
    {
        private readonly ILogger<ReceitaParserService> logger;
        private readonly DespesaReceitaService despesaReceitaService;

        public ReceitaParserService(
            ILogger<ReceitaParserService> logger,
            DespesaReceitaService despesaReceitaService)
        {
            this.logger = logger;
            this.despesaReceitaService = despesaReceitaService;
        }

        public async Task ParseObject(ReceitasCSV record)
        {
            var receita = new Receita
            {
                EspecieReceita = record.EspecieReceita,
                MunicipioDoador = record.NomeMunicipioDoador,
                SgPartido = record.SiglaPartido,
                FonteReceita = record.FonteReceita,
                NaturezaReceita = record.NaturezaReceita,
                Ano = record.AnoEleicao,
                Descricao = record.DescricaoReceita,
                NomeDoador = record.NomeDoador,
                NomeDoadorRFB = record.NomeDoadorRFB,
                OrigemReceita = record.OrigemReceita,
                Turno = int.Parse(record.Turno),
                SqCandidato = record.SequencialCandidato,
                Valor = record.ValorReceita / 100
            };

            if (DateTime.TryParse(record.DataReceita, out var dataReceita))
            {
                receita.DataReceita = dataReceita;
            }
            else
            {
                receita.DataReceita = null;
            }

            if (record.CpfCnpjDoador.Length == 0 || record.CpfCnpjDoador == "-4") {
                receita.CpfDoador = null;
                receita.CnpjDoador = null;
            }
            else if (record.CpfCnpjDoador.Length == 11)
            {
                receita.CpfDoador = record.CpfCnpjDoador;
                receita.CnpjDoador = null;
            }
            else if (record.CpfCnpjDoador.Length == 14)
            {
                receita.CnpjDoador = record.CpfCnpjDoador;
                receita.CpfDoador = null;
            }

            if (receita.Descricao.IsNullOrEmpty())
                receita.Descricao = null;
            if (receita.MunicipioDoador.IsNullOrEmpty())
                receita.MunicipioDoador = null;
            if (receita.NomeDoador.IsNullOrEmpty())
                receita.NomeDoador = null;
            if (receita.NomeDoadorRFB.IsNullOrEmpty())
                receita.NomeDoadorRFB = null;
            if (receita.OrigemReceita.IsNullOrEmpty())
                receita.OrigemReceita = null;
            if (receita.CpfDoador.IsNullOrEmpty())
                receita.CpfDoador = null;
            if (receita.CnpjDoador.IsNullOrEmpty())
                receita.CnpjDoador = null;

            await despesaReceitaService.AddReceitaAsync(receita);
        }
    }
}
