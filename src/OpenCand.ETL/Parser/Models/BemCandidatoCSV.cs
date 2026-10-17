using CsvHelper.Configuration.Attributes;

namespace OpenCand.Parser.Models
{
    public class BemCandidatoCSV
    {
        [Name("ANO_ELEICAO")]
        public int AnoEleicao { get; set; }

        [Name("SG_UF")]
        public string SiglaUF { get; set; }

        [Name("SG_UE")]
        public string SiglaUE { get; set; }

        [Name("NM_UE")]
        public string NomeUE { get; set; }

        [Name("SQ_CANDIDATO")]
        public string SequencialCandidato { get; set; }

        [Name("NR_ORDEM_BEM_CANDIDATO", "NR_ORDEM_CANDIDATO")]
        public int NumeroOrdemBemCandidato { get; set; }

        [Name("DS_TIPO_BEM_CANDIDATO")]
        public string DescricaoTipoBemCandidato { get; set; }

        [Name("DS_BEM_CANDIDATO")]
        public string DescricaoBemCandidato { get; set; }

        [Name("VR_BEM_CANDIDATO")]
        public string ValorBemCandidato { get; set; }
    }
}
