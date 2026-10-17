using Microsoft.Extensions.Logging;
using OpenCand.Core.Models;
using OpenCand.ETL.Contracts;
using OpenCand.ETL.Extensions;
using OpenCand.ETL.Services;
using OpenCand.Parser.Models;

namespace OpenCand.ETL.Parser.ParserServices
{
    public class DespesaParserService : IParserService<DespesasCSV>
    {
        private readonly ILogger<DespesaParserService> logger;
        private readonly DespesaReceitaService despesaReceitaService;

        public DespesaParserService(
            ILogger<DespesaParserService> logger,
            DespesaReceitaService despesaReceitaService)
        {
            this.logger = logger;
            this.despesaReceitaService = despesaReceitaService;
        }

        public async Task ParseObject(DespesasCSV record)
        {
            var despesa = new Despesa
            {
                SgPartido = record.SiglaPartido,
                Ano = record.AnoEleicao,
                Turno = int.Parse(record.Turno),
                Descricao = record.DescricaoDespesa,
                OrigemDespesa = record.OrigemDespesa,
                MunicipioFornecedor = record.NomeMunicipioFornecedor,
                NomeFornecedor = record.NomeFornecedor,
                NomeFornecedorRFB = record.NomeFornecedorRFB,
                SqCandidato = record.SequencialCandidato,
                TipoDocumento = record.TipoDocumento,
                TipoFornecedor = record.TipoFornecedor,
                Valor = record.ValorDespesaContratada / 100
            };

            if (DateTime.TryParse(record.DataDespesa, out var dataDespesa))
            {
                despesa.DataDespesa = dataDespesa;
            }
            else
            {
                despesa.DataDespesa = null;
            }

            if (record.CpfCnpjFornecedor.Length == 0 || record.CpfCnpjFornecedor == "-4")
            {
                despesa.CpfFornecedor = null;
                despesa.CnpjFornecedor = null;
            }
            else if (record.CpfCnpjFornecedor.Length == 11)
            {
                despesa.CpfFornecedor = record.CpfCnpjFornecedor;
                despesa.CnpjFornecedor = null;
            }
            else if (record.CpfCnpjFornecedor.Length == 14)
            {
                despesa.CnpjFornecedor = record.CpfCnpjFornecedor;
                despesa.CpfFornecedor = null;
            }

            if (despesa.Descricao.IsNullOrEmpty())
                despesa.Descricao = null;
            if (despesa.OrigemDespesa.IsNullOrEmpty())
                despesa.OrigemDespesa = null;
            if (despesa.MunicipioFornecedor.IsNullOrEmpty())
                despesa.MunicipioFornecedor = null;
            if (despesa.NomeFornecedor.IsNullOrEmpty())
                despesa.NomeFornecedor = null;
            if (despesa.NomeFornecedorRFB.IsNullOrEmpty())
                despesa.NomeFornecedorRFB = null;
            if (despesa.TipoDocumento.IsNullOrEmpty())
                despesa.TipoDocumento = null;
            if (despesa.CpfFornecedor.IsNullOrEmpty())
                despesa.CpfFornecedor = null;
            if (despesa.CnpjFornecedor.IsNullOrEmpty())
                despesa.CnpjFornecedor = null;
            if (despesa.TipoFornecedor.IsNullOrEmpty())
                despesa.TipoFornecedor = null;

            await despesaReceitaService.AddDespesaAsync(despesa);
        }
    }
}
