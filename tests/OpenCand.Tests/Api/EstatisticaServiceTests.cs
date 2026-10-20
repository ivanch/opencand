using OpenCand.API.Model;
using OpenCand.API.Services;
using static OpenCand.API.Model.GetValueSumRequest;

namespace OpenCand.Tests.Api;

public class EstatisticaServiceTests
{
    private static GetValueSumRequest Request(string type = "bem", string groupBy = "candidato", GetValueSumRequestFilter? filter = null)
        => new() { Type = type, GroupBy = groupBy, Filter = filter };

    [Fact]
    public void CacheKey_DiffersWhenOnlyFilterValueDiffers()
    {
        var pt = EstatisticaService.BuildValueSumCacheKey(Request(filter: new() { Partido = "PT" }));
        var psdb = EstatisticaService.BuildValueSumCacheKey(Request(filter: new() { Partido = "PSDB" }));

        Assert.NotEqual(pt, psdb);
    }

    [Fact]
    public void CacheKey_DiffersByEveryFilterField()
    {
        var keys = new[]
        {
            EstatisticaService.BuildValueSumCacheKey(Request()),
            EstatisticaService.BuildValueSumCacheKey(Request(filter: new() { Partido = "PT" })),
            EstatisticaService.BuildValueSumCacheKey(Request(filter: new() { Uf = "SP" })),
            EstatisticaService.BuildValueSumCacheKey(Request(filter: new() { Ano = 2022 })),
            EstatisticaService.BuildValueSumCacheKey(Request(filter: new() { Cargo = "PREFEITO" })),
            EstatisticaService.BuildValueSumCacheKey(Request(type: "despesa")),
            EstatisticaService.BuildValueSumCacheKey(Request(groupBy: "partido")),
        };

        Assert.Equal(keys.Length, keys.Distinct().Count());
    }

    [Fact]
    public void CacheKey_IsCaseAndWhitespaceInsensitiveForEquivalentRequests()
    {
        var a = EstatisticaService.BuildValueSumCacheKey(Request(type: "BEM", filter: new() { Uf = " sp " }));
        var b = EstatisticaService.BuildValueSumCacheKey(Request(type: "bem", filter: new() { Uf = "SP" }));

        Assert.Equal(a, b);
    }

    [Fact]
    public void EnriquecimentoCacheKey_IsNormalizedAndDiffersByFilter()
    {
        var a = EstatisticaService.BuildEnriquecimentoCacheKey(new() { Uf = " sp ", Partido = " PT" });
        var b = EstatisticaService.BuildEnriquecimentoCacheKey(new() { Uf = "SP", Partido = "PT" });
        var c = EstatisticaService.BuildEnriquecimentoCacheKey(new() { Uf = "SP", Partido = "PT", Ano = 2022 });

        Assert.Equal(a, b);
        Assert.NotEqual(b, c);
        Assert.Equal(EstatisticaService.BuildEnriquecimentoCacheKey(null), EstatisticaService.BuildEnriquecimentoCacheKey(new()));
        Assert.NotEqual(EstatisticaService.BuildEnriquecimentoCacheKey(null),
            EstatisticaService.BuildValueSumCacheKey(Request()));
    }

    [Fact]
    public void BuildQuery_NeverInlinesFilterValues()
    {
        var malicious = "PT' OR 1=1; DROP TABLE candidato; --";
        var builder = new EstatisticaService.SqlQueryBuilder();

        var sql = builder.BuildQuery(Request(filter: new()
        {
            Partido = malicious,
            Uf = malicious,
            Cargo = malicious,
            Ano = 2022
        }));

        Assert.DoesNotContain("DROP TABLE", sql);
        Assert.DoesNotContain("OR 1=1", sql);
        Assert.Contains("@partido", sql);
        Assert.Contains(builder.GetParameters().Values, v => (v as string) == malicious);
    }

    [Fact]
    public void BuildQuery_NormalizesUfParameter()
    {
        var builder = new EstatisticaService.SqlQueryBuilder();

        builder.BuildQuery(Request(filter: new() { Uf = " sp " }));

        Assert.Contains("SP", builder.GetParameters().Values.OfType<string>());
    }

    [Theory]
    [InlineData("bem", "mv_bem_candidato")]
    [InlineData("despesa", "mv_despesas_candidato")]
    [InlineData("receita", "mv_receitas_candidato")]
    public void BuildQuery_SelectsFromTheMatchingMaterializedView(string type, string expectedView)
    {
        var sql = new EstatisticaService.SqlQueryBuilder().BuildQuery(Request(type: type));

        Assert.Contains($"FROM {expectedView} ", sql);
    }

    [Theory]
    [InlineData("nope", "candidato")]
    [InlineData("bem", "nope")]
    public void BuildQuery_RejectsUnknownTypeOrGroupBy(string type, string groupBy)
    {
        var builder = new EstatisticaService.SqlQueryBuilder();

        Assert.Throws<ArgumentException>(() => builder.BuildQuery(Request(type: type, groupBy: groupBy)));
    }
}
