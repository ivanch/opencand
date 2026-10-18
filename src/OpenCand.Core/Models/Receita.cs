namespace OpenCand.Core.Models
{
    public class Receita
    {
        public Guid IdReceita { get; set; }
        public Guid IdCandidato { get; set; }
        public int Ano { get; set; }
        public int Turno { get; set; }

        public string SqCandidato { get; set; }

        public string SgPartido { get; set; }
        public string FonteReceita { get; set; }
        public string OrigemReceita { get; set; }
        public string NaturezaReceita { get; set; }
        public string EspecieReceita { get; set; }
        public string CpfDoador { get; set; }
        public string CnpjDoador { get; set; }
        public string NomeDoador { get; set; }
        public string NomeDoadorRFB { get; set; }
        public string MunicipioDoador { get; set; }
        public DateTime? DataReceita { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
    }
}
