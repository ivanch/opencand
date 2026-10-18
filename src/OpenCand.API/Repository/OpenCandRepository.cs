using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using OpenCand.Core.Models;
using OpenCand.Repository;

namespace OpenCand.API.Repository
{    
    public class OpenCandRepository : BaseRepository
    {
        private readonly string fotosPath;

        /// <param name="fotosPath">Resolved photos folder (FotosSettings.Path).</param>
        public OpenCandRepository(IConfiguration configuration, string fotosPath, IMemoryCache? cache = null) : base(configuration, cache)
        {
            this.fotosPath = fotosPath;
        }

        // Folder names look like foto_cand2022; anything else in the photos folder is ignored.
        private static readonly System.Text.RegularExpressions.Regex FotoFolderRegex =
            new(@"foto_cand(\d{4})", System.Text.RegularExpressions.RegexOptions.Compiled);

        internal static List<int> ParseFotoYears(IEnumerable<string> directories)
        {
            return directories
                .Select(dir => FotoFolderRegex.Match(System.IO.Path.GetFileName(dir.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar))))
                .Where(m => m.Success)
                .Select(m => int.Parse(m.Groups[1].Value))
                .Distinct()
                .OrderByDescending(ano => ano)
                .ToList();
        }

        /// <summary>Latest ETL run id, or null when no run was recorded (or none yet).</summary>
        public async Task<int?> GetLatestEtlRunIdAsync()
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            return await connection.QueryFirstOrDefaultAsync<int?>("SELECT max(id) FROM etl_run;");
        }

        public async Task<OpenCandStats> GetOpenCandStatsAsync()
        {
            string cacheKey = GenerateCacheKey("OpenCandStats");
            
            var result = await GetOrSetCacheAsync(cacheKey, async () =>
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    var stats = await connection.QueryFirstOrDefaultAsync<OpenCandStats>(@"
                        SELECT 
                            (SELECT COUNT(idcandidato) FROM candidato) AS TotalCandidatos,
                            (SELECT COUNT(*) FROM bem_candidato) AS TotalBemCandidatos,
                            (SELECT SUM(valor) FROM bem_candidato) AS TotalValorBemCandidatos,
                            (SELECT COUNT(*) FROM rede_social) AS TotalRedesSociais,
                            (SELECT COUNT(DISTINCT ano) FROM candidato_mapping) AS TotalEleicoes;");
                    
                    return stats ?? new OpenCandStats();
                }
            });
            
            return result ?? new OpenCandStats();
        }
        
        public async Task<DataAvailabilityStats> GetDataAvailabilityAsync()
        {
            string cacheKey = GenerateCacheKey("DataAvailabilityStats");

            var result = await GetOrSetCacheAsync(cacheKey, async () =>
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    var stats = new DataAvailabilityStats();

                    // Get years for each data type separately
                    var candidatosYears = await connection.QueryAsync<int>("SELECT DISTINCT ano FROM candidato_mapping ORDER BY ano DESC");
                    var bemCandidatosYears = await connection.QueryAsync<int>("SELECT DISTINCT ano FROM bem_candidato ORDER BY ano DESC");
                    var despesaCandidatosYears = await connection.QueryAsync<int>("SELECT DISTINCT ano FROM despesas_candidato ORDER BY ano DESC");
                    var receitaCandidatosYears = await connection.QueryAsync<int>("SELECT DISTINCT ano FROM receitas_candidato ORDER BY ano DESC");
                    var redeSocialCandidatosYears = await connection.QueryAsync<int>("SELECT DISTINCT ano FROM rede_social ORDER BY ano DESC");

                    stats.Candidatos = candidatosYears.ToList();
                    stats.BemCandidatos = bemCandidatosYears.ToList();
                    stats.DespesaCandidatos = despesaCandidatosYears.ToList();
                    stats.ReceitaCandidatos = receitaCandidatosYears.ToList();
                    stats.RedeSocialCandidatos = redeSocialCandidatosYears.ToList();

                    if (Directory.Exists(fotosPath))
                        stats.FotosCandidatos = ParseFotoYears(Directory.GetDirectories(fotosPath));

                    return stats;
                }
            });

            return result ?? new DataAvailabilityStats();
        }
        
        public async Task<DatabaseTechStats> GetDatabaseTechStatsAsync()
        {
            string cacheKey = GenerateCacheKey("DatabaseTechStats");

            var result = await GetOrSetCacheAsync(cacheKey, async () =>
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    var stats = new DatabaseTechStats();


                    // Get table stats using reltuples for entries
                    var tableStats = await connection.QueryAsync<TableStats>(@"
                        SELECT 
                            pt.schemaname||'.'||pt.tablename as Name,
                            pg_total_relation_size(pt.schemaname||'.'||pt.tablename) as TotalSize,
                            COALESCE(c.reltuples,0)::bigint as Entries
                        FROM pg_tables pt
                        JOIN pg_class c ON c.relname = pt.tablename AND c.relkind = 'r'
                        WHERE pt.schemaname = 'public'
                        ORDER BY pg_total_relation_size(pt.schemaname||'.'||pt.tablename) DESC;");


                    var tableStatsList = tableStats.ToList();
                    stats.Tables = tableStatsList;
                    stats.TotalSize = tableStatsList.Sum(t => t.TotalSize);
                    stats.TotalEntries = tableStatsList.Sum(t => t.Entries);

                    // Get materialized view stats using reltuples for entries
                    var materializedViewStats = await connection.QueryAsync<TableStats>(@"
                        SELECT 
                            pmv.schemaname||'.'||pmv.matviewname as Name,
                            pg_total_relation_size(pmv.schemaname||'.'||pmv.matviewname) as TotalSize,
                            COALESCE(c.reltuples,0)::bigint as Entries
                        FROM pg_matviews pmv
                        JOIN pg_class c ON c.relname = pmv.matviewname AND c.relkind = 'm'
                        WHERE pmv.schemaname = 'public'
                        ORDER BY pg_total_relation_size(pmv.schemaname||'.'||pmv.matviewname) DESC;");

                    stats.MaterializedViews = materializedViewStats.ToList();

                    // Get index stats
                    var indexStats = await connection.QueryFirstOrDefaultAsync<IndexStats>(@"
                        SELECT 
                            COUNT(*) as Amount,
                            SUM(pg_relation_size(indexrelid)) as Size
                        FROM pg_stat_user_indexes
                        WHERE schemaname = 'public';");

                    stats.Indexes = indexStats ?? new IndexStats();

                    return stats;
                }
            });

            return result ?? new DatabaseTechStats();
        }
    }
}
