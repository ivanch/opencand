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

    /// <summary>
    /// Page and PageSize are 1 and Total when the caller did not ask for paging (legacy, un-paginated response).
    /// </summary>
    public class DespesasResult
    {
        public List<Despesa> Despesas { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; }
        public long Total { get; set; }
    }

    public class ReceitaResult
    {
        public List<Receita> Receitas { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; }
        public long Total { get; set; }
    }

    /// <summary>Totals and row counts of despesas or receitas for one election year.</summary>
    public class AnoResumo
    {
        public int Ano { get; set; }
        public long Quantidade { get; set; }
        public decimal Total { get; set; }
    }

    public class FinanceResumoResult
    {
        public List<AnoResumo> Resumo { get; set; } = new();
    }
}
