namespace OpenCand.Core.Models
{
    public class OpenCandStats
    {
        public long TotalCandidatos { get; set; }
        public long TotalBemCandidatos { get; set; }
        public long TotalValorBemCandidatos { get; set; }
        public long TotalRedesSociais { get; set; }
        public long TotalEleicoes { get; set; }
    }

    public class DataAvailabilityStats
    {
        public List<int> Candidatos { get; set; }
        public List<int> BemCandidatos { get; set; }
        public List<int> DespesaCandidatos { get; set; }
        public List<int> ReceitaCandidatos { get; set; }
        public List<int> RedeSocialCandidatos { get; set; }
        
        public List<int> FotosCandidatos { get; set; }
    }
}
