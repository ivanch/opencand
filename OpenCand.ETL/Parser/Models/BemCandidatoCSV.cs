using CsvHelper.Configuration.Attributes;

namespace OpenCand.Parser.Models
{
    public class BemCandidatoCSV
    {
        [Name("DT_GERACAO")]
        public string DataGeracao { get; set; }

        [Name("HH_GERACAO")]
        public string HoraGeracao { get; set; }

        [Name("ANO_ELEICAO")]
        public int AnoEleicao { get; set; }

        [Name("CD_TIPO_ELEICAO")]
        public int CodigoTipoEleicao { get; set; }

        [Name("NM_TIPO_ELEICAO")]
        public string NomeTipoEleicao { get; set; }

        [Name("CD_ELEICAO")]
        public int CodigoEleicao { get; set; }

        [Name("DS_ELEICAO")]
        public string DescricaoEleicao { get; set; }

        [Name("DT_ELEICAO")]
        public string DataEleicao { get; set; }

        [Name("SG_UF")]
        public string SiglaUF { get; set; }

        [Name("SG_UE")]
        public string SiglaUE { get; set; }

        [Name("NM_UE")]
        public string NomeUE { get; set; }

        [Name("SQ_CANDIDATO")]
        public string SequencialCandidato { get; set; }

        [Name("NR_ORDEM_BEM_CANDIDATO")]
        public int NumeroOrdemBemCandidato { get; set; }

        [Name("CD_TIPO_BEM_CANDIDATO")]
        public int CodigoTipoBemCandidato { get; set; }

        [Name("DS_TIPO_BEM_CANDIDATO")]
        public string DescricaoTipoBemCandidato { get; set; }

        [Name("DS_BEM_CANDIDATO")]
        public string DescricaoBemCandidato { get; set; }

        [Name("VR_BEM_CANDIDATO")]
        public string ValorBemCandidato { get; set; }
    }
}
