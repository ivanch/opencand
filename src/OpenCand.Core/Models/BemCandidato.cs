namespace OpenCand.Core.Models
{
    public class BemCandidato
    {
        public Guid IdCandidato { get; set; }

        // Only for fetching purposes
        public string? SqCandidato { get; set; }

        public int Ano { get; set; }

        public string SiglaUF { get; set; }

        public string NomeUE { get; set; }

        public int OrdemBem { get; set; }
        
        public string? TipoBem { get; set; }
        
        public string? Descricao { get; set; }
        
        public decimal? Valor { get; set; }
    }
}
