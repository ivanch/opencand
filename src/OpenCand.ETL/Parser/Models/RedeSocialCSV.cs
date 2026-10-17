using CsvHelper.Configuration.Attributes;

namespace OpenCand.Parser.Models
{
    public class RedeSocialCSV
    {
        [Name("AA_ELEICAO")]
        public int DataEleicao { get; set; }

        [Name("SG_UF")]
        public string SiglaUF { get; set; }

        [Name("SQ_CANDIDATO")]
        public string SequencialCandidato { get; set; }

        [Name("DS_URL")]
        public string Url { get; set; }
    }
}
