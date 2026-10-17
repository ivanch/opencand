namespace OpenCand.Core.Models
{
    public class Despesa
    {
        public Guid IdDespesa { get; set; }
        public Guid IdCandidato { get; set; }
        public int Ano { get; set; }
        public int Turno { get; set; }

        public string SqCandidato { get; set; }

        public string SgPartido { get; set; }
        public string TipoFornecedor { get; set; }
        public string CpfFornecedor { get; set; }
        public string CnpjFornecedor { get; set; }
        public string NomeFornecedor { get; set; }
        public string NomeFornecedorRFB { get; set; }
        public string MunicipioFornecedor { get; set; }
        public string TipoDocumento { get; set; }
        public DateTime? DataDespesa { get; set; }
        public string OrigemDespesa { get; set; }
        public string Descricao { get; set; }
        public float Valor { get; set; }
    }
}
