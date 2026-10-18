namespace OpenCand.API.Model
{
    public class MaioresEnriquecimento
    {
        public Guid IdCandidato { get; set; }
        public string Nome { get; set; }
        public decimal PatrimonioInicial { get; set; }
        public int AnoInicial { get; set; }
        public decimal PatrimonioFinal { get; set; }
        public int AnoFinal { get; set; }
        public decimal Enriquecimento { get; set; }
    }

    public class GetValueSumRequest
    {
        public string Type { get; set; } // "bem", "despesa", "receita"
        public string GroupBy { get; set; } // "candidato", "partido", "uf", or "cargo"
        public GetValueSumRequestFilter? Filter { get; set; }
        public class GetValueSumRequestFilter
        {
            public string? Partido { get; set; } // Optional, can be null
            public string? Uf { get; set; } // Optional, can be null
            public int? Ano { get; set; } // Optional, can be null
            public string? Cargo { get; set; } // Optional, can be null
        }
    }

    public class GetValueSumResponse
    {
        public Guid? IdCandidato { get; set; }
        public string? Sgpartido { get; set; }
        public string? SiglaUf { get; set; }
        public string? Cargo { get; set; }
        public string? Nome { get; set; }
        public int Ano { get; set; }
        public decimal Valor { get; set; }
    }
}
