using System;
using CsvHelper.Configuration.Attributes;

namespace OpenCand.Parser.Models
{
    public class CandidatoCSV
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

        [Name("NR_TURNO")]
        public int NumeroTurno { get; set; }

        [Name("CD_ELEICAO")]
        public int CodigoEleicao { get; set; }

        [Name("DS_ELEICAO")]
        public string DescricaoEleicao { get; set; }

        [Name("DT_ELEICAO")]
        public string DataEleicao { get; set; }

        [Name("TP_ABRANGENCIA")]
        public string TipoAbrangencia { get; set; }

        [Name("SG_UF")]
        public string SiglaUF { get; set; }

        [Name("SG_UE")]
        public string SiglaUE { get; set; }

        [Name("NM_UE")]
        public string NomeUE { get; set; }

        [Name("CD_CARGO")]
        public int CodigoCargo { get; set; }

        [Name("DS_CARGO")]
        public string DescricaoCargo { get; set; }

        [Name("SQ_CANDIDATO")]
        public string SequencialCandidato { get; set; }

        [Name("NR_CANDIDATO")]
        public string NumeroCandidato { get; set; }

        [Name("NM_CANDIDATO")]
        public string NomeCandidato { get; set; }

        [Name("NM_URNA_CANDIDATO")]
        public string NomeUrnaCandidato { get; set; }

        [Name("NM_SOCIAL_CANDIDATO")]
        public string NomeSocialCandidato { get; set; }

        [Name("NR_CPF_CANDIDATO")]
        public string CPFCandidato { get; set; }

        [Name("DS_EMAIL", "NM_EMAIL")]
        public string Email { get; set; }

        [Name("SG_UF_NASCIMENTO")]
        public string SiglaUFNascimento { get; set; }

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
    }
}
