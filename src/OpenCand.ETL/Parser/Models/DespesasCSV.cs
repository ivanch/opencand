using CsvHelper.Configuration.Attributes;

namespace OpenCand.Parser.Models
{
    public class DespesasCSV
    {
        [Name("AA_ELEICAO")]
        public int AnoEleicao { get; set; }

        [Name("ST_TURNO")]
        public string Turno { get; set; }

        [Name("DT_PRESTACAO_CONTAS")]
        public string DataPrestacaoContas { get; set; }

        [Name("SQ_PRESTADOR_CONTAS")]
        public string SequencialPrestadorContas { get; set; }

        [Name("SG_UF")]
        public string SiglaUF { get; set; }

        [Name("NM_UE")]
        public string NomeUE { get; set; }

        [Name("NR_CNPJ_PRESTADOR_CONTA")]
        public string CnpjPrestadorConta { get; set; }

        [Name("SQ_CANDIDATO")]
        public string SequencialCandidato { get; set; }

        [Name("NR_CPF_CANDIDATO")]
        public string CpfCandidato { get; set; }

        [Name("SG_PARTIDO")]
        public string SiglaPartido { get; set; }

        [Name("DS_TIPO_FORNECEDOR")]
        public string TipoFornecedor { get; set; }

        [Name("CD_CNAE_FORNECEDOR")]
        public string CodigoCnaeFornecedor { get; set; }

        [Name("DS_CNAE_FORNECEDOR")]
        public string DescricaoCnaeFornecedor { get; set; }

        [Name("NR_CPF_CNPJ_FORNECEDOR")]
        public string CpfCnpjFornecedor { get; set; }

        [Name("NM_FORNECEDOR")]
        public string NomeFornecedor { get; set; }

        [Name("NM_FORNECEDOR_RFB")]
        public string NomeFornecedorRFB { get; set; }

        [Name("SG_UF_FORNECEDOR")]
        public string SiglaUFFornecedor { get; set; }

        [Name("NM_MUNICIPIO_FORNECEDOR")]
        public string NomeMunicipioFornecedor { get; set; }

        [Name("SQ_CANDIDATO_FORNECEDOR")]
        public string SequencialCandidatoFornecedor { get; set; }

        [Name("NR_CANDIDATO_FORNECEDOR")]
        public string NumeroCandidatoFornecedor { get; set; }

        [Name("DS_CARGO_FORNECEDOR")]
        public string CargoFornecedor { get; set; }

        [Name("NR_PARTIDO_FORNECEDOR")]
        public string NumeroPartidoFornecedor { get; set; }

        [Name("SG_PARTIDO_FORNECEDOR")]
        public string SiglaPartidoFornecedor { get; set; }

        [Name("NM_PARTIDO_FORNECEDOR")]
        public string NomePartidoFornecedor { get; set; }

        [Name("DS_TIPO_DOCUMENTO")]
        public string TipoDocumento { get; set; }

        [Name("DS_ORIGEM_DESPESA")]
        public string OrigemDespesa { get; set; }

        [Name("SQ_DESPESA")]
        public string SequencialDespesa { get; set; }

        [Name("DT_DESPESA")]
        public string DataDespesa { get; set; }

        [Name("DS_DESPESA")]
        public string DescricaoDespesa { get; set; }

        [Name("VR_DESPESA_CONTRATADA")]
        public float ValorDespesaContratada { get; set; }
    }
}
