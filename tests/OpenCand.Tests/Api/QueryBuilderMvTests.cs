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
    public void BuildQuery_GroupByCandidato_JoinsCandidatoOnlyForTheName()
    {
        var sql = new EstatisticaService.SqlQueryBuilder().BuildQuery(Request(groupBy: "candidato"));

        Assert.Contains("JOIN candidato c ON c.idcandidato = src.idcandidato", sql);
        Assert.Contains("SELECT src.idcandidato, c.nome, src.ano, SUM(src.valor)", sql);
        Assert.Contains("GROUP BY src.idcandidato, c.nome, src.ano", sql);
        Assert.Equal(1, CountOccurrences(sql, "JOIN"));
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
