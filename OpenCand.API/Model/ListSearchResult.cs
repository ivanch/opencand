using OpenCand.Core.Models;

namespace OpenCand.API.Model
{
    public class CandidatoSearchResult
    {
        public List<Candidato> Candidatos { get; set; }
    }

    public class BemCandidatoResult
    {
        public List<BemCandidato> Bens { get; set; }
    }

    public class RedeSocialResult
    {
        public List<RedeSocial> RedesSociais { get; set; }
    }

    public class CpfRevealResult
    {
        public string Cpf { get; set; }
    }
}
