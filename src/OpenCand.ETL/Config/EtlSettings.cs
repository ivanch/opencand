namespace OpenCand.Config
{
    public class EtlSettings
    {
        /// <summary>Rows buffered in memory before a candidatos/bens/redes flush (COPY into temp table + upsert).</summary>
        public int BatchSize { get; set; } = 50_000;

        /// <summary>CSV files loaded concurrently for despesas/receitas (each file streams into its own COPY).</summary>
        public int MaxParallelFiles { get; set; } = 4;

        /// <summary>A dataset fails the run when rejected rows / read rows is above this ratio.</summary>
        public double MaxRejectRatio { get; set; } = 0.01;

        public string CsvEncoding { get; set; } = "windows-1252";

        /// <summary>Force dropping and re-creating the materialized views even if no table was swapped.</summary>
        public bool RebuildViews { get; set; }
    }
}
