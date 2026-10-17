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
        public List<int> Candidatos { get; set; } = new List<int>();
        public List<int> BemCandidatos { get; set; } = new List<int>();
        public List<int> DespesaCandidatos { get; set; } = new List<int>();
        public List<int> ReceitaCandidatos { get; set; } = new List<int>();
        public List<int> RedeSocialCandidatos { get; set; } = new List<int>();

        public List<int> FotosCandidatos { get; set; } = new List<int>();
    }

    public class DatabaseTechStats
    {
        public List<TableStats> Tables { get; set; } = new List<TableStats>();
        public List<TableStats> MaterializedViews { get; set; } = new List<TableStats>();
        public IndexStats Indexes { get; set; } = new IndexStats();
        public long TotalSize { get; set; }
        public long TotalEntries { get; set; }
    }

    public class TableStats
    {
        public string Name { get; set; } = string.Empty;
        public long TotalSize { get; set; }
        public long Entries { get; set; }
    }

    public class IndexStats
    {
        public int Amount { get; set; }
        public long Size { get; set; }
    }
}
