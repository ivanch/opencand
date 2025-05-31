using System;
using CsvHelper.Configuration.Attributes;

namespace OpenCand.Parser.Models
{
    public class CandidatoCSV
    {
        [Name("ANO_ELEICAO")]
        public int AnoEleicao { get; set; }

        [Name("TP_ABRANGENCIA")]
        public string TipoAbrangencia { get; set; }

        [Name("SG_UF")]
        public string SiglaUF { get; set; }

        [Name("NM_UE")]
        public string NomeUE { get; set; }

        [Name("DS_CARGO")]
        public string DescricaoCargo { get; set; }

        [Name("SQ_CANDIDATO")]
        public string SequencialCandidato { get; set; }

        [Name("NR_CANDIDATO")]
        public string NumeroCandidato { get; set; }

        [Name("NM_CANDIDATO")]
        public string NomeCandidato { get; set; }

        [Name("NR_CPF_CANDIDATO")]
        public string CPFCandidato { get; set; }

        [Name("DS_EMAIL", "NM_EMAIL")]
        public string Email { get; set; }

        [Name("DT_NASCIMENTO")]
        public string DataNascimento { get; set; }

        [Name("DS_GENERO")]
        public string Genero { get; set; }

        [Name("DS_OCUPACAO")]
        public string Ocupacao { get; set; }

        [Name("DS_ESTADO_CIVIL")]
        public string EstadoCivil { get; set; }

        [Name("DS_GRAU_INSTRUCAO")]
        public string GrauInstrucao { get; set; }

        [Name("DS_SIT_TOT_TURNO")]
        public string SituacaoTurno { get; set; }

        [Name("NR_PARTIDO")]
        public int NumeroPartido { get; set; }

        [Name("SG_PARTIDO")]
        public string SiglaPartido { get; set; }

        [Name("NM_PARTIDO")]
        public string NomePartido { get; set; }
    }
}
