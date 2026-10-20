using OpenCand.ETL.Database;

namespace OpenCand.Tests.Etl;

public class SwapTableTests
{
    [Fact]
    public void CutoverRestoresTheCanonicalNamesOfDbSql()
    {
        var statements = SwapTable.Despesas.CutoverStatements().ToList();

        Assert.Equal("DROP TABLE despesas_candidato", statements[0]);
        Assert.Contains("ALTER TABLE despesas_candidato_new RENAME TO despesas_candidato", statements);
        Assert.Contains("ALTER TABLE despesas_candidato RENAME CONSTRAINT pk_despesas_candidato_new TO pk_despesas_candidato", statements);
        Assert.Contains("ALTER TABLE despesas_candidato RENAME CONSTRAINT fk_despesas_candidato_candidato_new TO fk_despesas_candidato_candidato", statements);
        Assert.Contains("ALTER INDEX idx_despesas_candidato_sgpartido_new RENAME TO idx_despesas_candidato_sgpartido", statements);
    }

    [Fact]
    public void EmbeddedMaterializedViewSqlMatchesDbMvSql()
    {
        var embedded = MaterializedViews.LoadSql();

        Assert.Contains("CREATE MATERIALIZED VIEW mv_bem_candidato", embedded);
        Assert.Contains("CREATE MATERIALIZED VIEW mv_despesas_candidato", embedded);
        Assert.Contains("CREATE UNIQUE INDEX", embedded);
    }

    [Theory]
    [InlineData("mv_bem_resumo", "mv_bem_candidato")]
    [InlineData("mv_despesas_resumo", "mv_despesas_candidato")]
    [InlineData("mv_receitas_resumo", "mv_receitas_candidato")]
    [InlineData("mv_enriquecimento_candidato", "mv_bem_candidato_ano")]
    public void DependentViewsAreDroppedBeforeAndCreatedAfterTheirSource(string view, string source)
    {
        var sql = MaterializedViews.LoadSql();

        Assert.True(sql.IndexOf($"DROP MATERIALIZED VIEW IF EXISTS {view};", StringComparison.Ordinal)
                    < sql.IndexOf($"DROP MATERIALIZED VIEW IF EXISTS {source};", StringComparison.Ordinal));
        Assert.True(sql.IndexOf($"CREATE MATERIALIZED VIEW {source} ", StringComparison.Ordinal)
                    < sql.IndexOf($"CREATE MATERIALIZED VIEW {view} ", StringComparison.Ordinal));
        Assert.Contains($"CREATE UNIQUE INDEX ux_{view} ON {view}", sql);
    }
}
