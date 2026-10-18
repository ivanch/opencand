using CsvHelper.Configuration.Attributes;

namespace OpenCand.Parser.Models
{
    public class ReceitasCSV
    {
        [Name("AA_ELEICAO")]
        public int AnoEleicao { get; set; }

        [Name("ST_TURNO")]
        public string Turno { get; set; }

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

        [Name("DS_FONTE_RECEITA")]
        public string FonteReceita { get; set; }

        [Name("DS_ORIGEM_RECEITA")]
        public string OrigemReceita { get; set; }

        [Name("DS_NATUREZA_RECEITA")]
        public string NaturezaReceita { get; set; }

        [Name("DS_ESPECIE_RECEITA")]
        public string EspecieReceita { get; set; }

        [Name("CD_CNAE_DOADOR")]
        public string CodigoCnaeDoador { get; set; }

        [Name("DS_CNAE_DOADOR")]
        public string DescricaoCnaeDoador { get; set; }

        [Name("NR_CPF_CNPJ_DOADOR")]
        public string CpfCnpjDoador { get; set; }

        [Name("NM_DOADOR")]
        public string NomeDoador { get; set; }

        [Name("NM_DOADOR_RFB")]
        public string NomeDoadorRFB { get; set; }

        [Name("SG_UF_DOADOR")]
        public string SiglaUFDoaror { get; set; }

        [Name("NM_MUNICIPIO_DOADOR")]
        public string NomeMunicipioDoador { get; set; }

        [Name("SQ_CANDIDATO_DOADOR")]
        public string SequencialCandidatoDoador { get; set; }

        [Name("SG_PARTIDO_DOADOR")]
        public string SiglaPartidoDoador { get; set; }

        [Name("NR_RECIBO_DOACAO")]
        public string NumeroReciboDoacao { get; set; }

        [Name("NR_DOCUMENTO_DOACAO")]
        public string NumeroDocumentoDoacao { get; set; }

        [Name("SQ_RECEITA")]
        public string SequencialReceita { get; set; }

        [Name("DT_RECEITA")]
        public string DataReceita { get; set; }

        [Name("DS_RECEITA")]
        public string DescricaoReceita { get; set; }

        [Name("VR_RECEITA")]
        public string? ValorReceita { get; set; }

        [Name("DS_GENERO")]
        public string Genero { get; set; }
    }
}
