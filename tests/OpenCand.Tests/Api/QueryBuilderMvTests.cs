using OpenCand.API.Model;
using OpenCand.API.Services;
using static OpenCand.API.Model.GetValueSumRequest;

namespace OpenCand.Tests.Api;

public class QueryBuilderMvTests
{
    private static GetValueSumRequest Request(string type = "bem", string groupBy = "candidato", GetValueSumRequestFilter? filter = null)
        => new() { Type = type, GroupBy = groupBy, Filter = filter };

    private static GetValueSumRequestFilter AllFilters => new() { Partido = "PT", Uf = "SP", Ano = 2022, Cargo = "DEPUTADO" };

    [Theory]
    [InlineData("candidato")]
    [InlineData("partido")]
    [InlineData("uf")]
    [InlineData("cargo")]
    public void BuildQuery_NeverJoinsTheMappingAnalyticsView(string groupBy)
    {
        var sql = new EstatisticaService.SqlQueryBuilder().BuildQuery(Request(groupBy: groupBy, filter: AllFilters));

        Assert.DoesNotContain("mv_candidato_mapping_analytics", sql);
        Assert.DoesNotContain("cm.", sql);
    }

    [Fact]
    public void BuildQuery_FiltersOnSourceColumns()
    {
        var sql = new EstatisticaService.SqlQueryBuilder().BuildQuery(Request(groupBy: "partido", filter: AllFilters));

        Assert.Contains("src.sgpartido = @", sql);
        Assert.Contains("src.siglauf = @", sql);
        Assert.Contains("src.ano = @", sql);
        Assert.Contains("src.cargo = @", sql);
    }

    [Fact]
    public void BuildQuery_GroupByCandidato_JoinsCandidatoOnlyForTheTopTen()
    {
        var sql = new EstatisticaService.SqlQueryBuilder().BuildQuery(Request(groupBy: "candidato", filter: AllFilters));

        // aggregate + LIMIT inside the subquery, the join to candidato only outside it
        var inner = sql[sql.IndexOf("(SELECT", StringComparison.Ordinal)..(sql.IndexOf(") t ", StringComparison.Ordinal) + 1)];
        Assert.Contains("SELECT src.idcandidato, src.ano, SUM(src.valor)", inner);
        Assert.Contains("FROM mv_bem_candidato src WHERE", inner);
        Assert.Contains("GROUP BY src.idcandidato, src.ano", inner);
        Assert.EndsWith("LIMIT 10)", inner);
        Assert.DoesNotContain("JOIN", inner);

        Assert.StartsWith("SELECT t.idcandidato, c.nome, t.ano, t.valor FROM (", sql);
        Assert.EndsWith("JOIN candidato c ON c.idcandidato = t.idcandidato ORDER BY t.valor DESC", sql);
        Assert.Equal(1, CountOccurrences(sql, "JOIN"));
    }

    [Theory]
    [InlineData("bem", "partido", "mv_bem_resumo")]
    [InlineData("despesa", "uf", "mv_despesas_resumo")]
    [InlineData("receita", "cargo", "mv_receitas_resumo")]
    public void BuildQuery_GroupByDimension_UsesTheRollupView(string type, string groupBy, string expectedView)
    {
        var sql = new EstatisticaService.SqlQueryBuilder().BuildQuery(Request(type: type, groupBy: groupBy, filter: AllFilters));

        Assert.Contains($"FROM {expectedView} src WHERE", sql);
        Assert.DoesNotContain("_candidato", sql);
    }

    [Theory]
    [InlineData("partido", "src.sgpartido")]
    [InlineData("uf", "src.siglauf")]
    [InlineData("cargo", "src.cargo")]
    public void BuildQuery_GroupByDimension_UsesSourceColumnWithoutAnyJoin(string groupBy, string column)
    {
        var sql = new EstatisticaService.SqlQueryBuilder().BuildQuery(Request(groupBy: groupBy));

        Assert.DoesNotContain("JOIN", sql);
        Assert.Contains($"SELECT {column}, src.ano", sql);
        Assert.Contains($"GROUP BY {column}, src.ano", sql);
    }

    [Fact]
    public void BuildQuery_WithoutFilter_HasNoWhereClause()
    {
        var sql = new EstatisticaService.SqlQueryBuilder().BuildQuery(Request(groupBy: "uf"));

        Assert.DoesNotContain("WHERE", sql);
        Assert.EndsWith("ORDER BY SUM(src.valor) DESC LIMIT 10", sql);
    }

    private static int CountOccurrences(string text, string value)
        => (text.Length - text.Replace(value, "").Length) / value.Length;
}
