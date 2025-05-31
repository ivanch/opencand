using System.IO.Pipes;

namespace OpenCand.Core.Models
{
    public class Candidato
    {
        public Guid IdCandidato { get; set; }

        public string Cpf { get; set; }

        public string SqCandidato { get; set; }

        public string Nome { get; set; }

        public DateTime? DataNascimento { get; set; }

        public string Email { get; set; }

        public string Sexo { get; set; }

        public string EstadoCivil { get; set; }

        public string Escolaridade { get; set; }

        public string Ocupacao { get; set; }

        public List<CandidatoMapping> Eleicoes { get; set; }

        // API ONLY
        public string FotoUrl { get; set; }
    }

    public class CandidatoMapping
    {
        public Guid IdCandidato { get; set; }
        public string Cpf { get; set; }
        public string Nome { get; set; }
        public string SqCandidato { get; set; }
        public int Ano { get; set; }
        public string TipoEleicao { get; set; }
        public string SiglaUF { get; set; }
        public string NomeUE { get; set; }
        public string Cargo { get; set; }
        public string NrCandidato { get; set; }
        public string Resultado { get; set; }
        public string Sgpartido { get; set; }

        public Partido? Partido { get; set; } // Nullable to allow for candidates without a party
    }

    public class RedeSocial
    {
        public Guid IdCandidato { get; set; }
        public string SqCandidato { get; set; }

        public string Rede { get; set; }
        public string SiglaUF { get; set; }
        public int Ano { get; set; }
        public string Link { get; set; }
    }
}
