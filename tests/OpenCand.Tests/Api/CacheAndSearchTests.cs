using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using OpenCand.API.Repository;
using OpenCand.Repository;

namespace OpenCand.Tests.Api;

public class CacheAndSearchTests
{
    [Theory]
    [InlineData("  Lula ", "lula")]
    [InlineData("LULA DA SILVA", "lula da silva")]
    [InlineData("lula", "lula")]
    public void NormalizeSearchQuery_TrimsAndLowerCases(string input, string expected)
    {
        Assert.Equal(expected, CandidatoRepository.NormalizeSearchQuery(input));
    }

    [Fact]
    public void SearchCacheKey_IsSharedByEquivalentQueries()
    {
        var a = CandidatoRepository.SearchCacheKey(CandidatoRepository.NormalizeSearchQuery(" Lula"));
        var b = CandidatoRepository.SearchCacheKey(CandidatoRepository.NormalizeSearchQuery("lula  "));

        Assert.Equal(a, b);
    }

    [Fact]
    public void CacheTtls_AreSixHoursByDefaultAndTenMinutesForSearch()
    {
        Assert.Equal(TimeSpan.FromHours(6), BaseRepository.DefaultCacheExpiration);
        Assert.Equal(TimeSpan.FromMinutes(10), BaseRepository.SearchCacheExpiration);
    }

    [Fact]
    public void ParseFotoYears_IgnoresUnexpectedFolderNames()
    {
        var years = OpenCandRepository.ParseFotoYears(new[]
        {
            "/data/fotos/foto_cand2018",
            "/data/fotos/foto_cand2022/",
            "/data/fotos/foto_cand2022",
            "/data/fotos/lost+found",
            "/data/fotos/foto_cand",
            "/data/fotos/foto_candidatos_old",
        });

        Assert.Equal(new[] { 2022, 2018 }, years);
    }

    // The cache is limited by entry count, so a test with SizeLimit = 2 proves each entry costs exactly 1.
    private sealed class ProbeRepository : BaseRepository
    {
        public ProbeRepository(IMemoryCache cache) : base(
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseSettings:ConnectionString"] = "Host=unused"
            }).Build(), cache)
        {
        }

        public Task<string?> Get(string key, Func<string> factory, TimeSpan? ttl = null)
            => GetOrSetCacheAsync<string>(key, () => Task.FromResult<string?>(factory()), ttl);
    }

    [Fact]
    public async Task Cache_CountsEntriesNotBytes()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 2 });
        var repo = new ProbeRepository(cache);

        await repo.Get("a", () => new string('x', 100_000));
        await repo.Get("b", () => new string('y', 100_000));
        await repo.Get("c", () => "z"); // over the 2-entry limit: not stored

        Assert.Equal(2, cache.Count);
        Assert.True(cache.TryGetValue("a", out _));
        Assert.True(cache.TryGetValue("b", out _));
    }

    [Fact]
    public async Task Cache_ServesSecondCallFromCache()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 10 });
        var repo = new ProbeRepository(cache);
        var calls = 0;

        await repo.Get("k", () => $"v{++calls}");
        var second = await repo.Get("k", () => $"v{++calls}");

        Assert.Equal("v1", second);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Cache_EntriesHaveAnAbsoluteExpiration()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 10 });
        var repo = new ProbeRepository(cache);

        await repo.Get("k", () => "v", TimeSpan.FromMilliseconds(50));
        await Task.Delay(150);

        Assert.False(cache.TryGetValue("k", out _));
    }

    [Fact]
    public async Task Cache_CompactRemovesEverything()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 10 });
        var repo = new ProbeRepository(cache);
        await repo.Get("a", () => "1");
        await repo.Get("b", () => "2");

        cache.Compact(1.0);

        Assert.Equal(0, cache.Count);
    }
}
