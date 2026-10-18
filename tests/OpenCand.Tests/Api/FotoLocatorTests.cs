using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenCand.API.Config;
using OpenCand.API.Services;

namespace OpenCand.Tests.Api;

public class FotoLocatorTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "opencand-fotos-" + Guid.NewGuid());

    public FotoLocatorTests() => Directory.CreateDirectory(root);

    public void Dispose() => Directory.Delete(root, recursive: true);

    private sealed class Env : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Test";
        public string ApplicationName { get; set; } = "Test";
        public string ContentRootPath { get; set; } = "/";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private FotoLocator Create(IMemoryCache? cache = null) => new(
        Options.Create(new FotosSettings { Path = root, ApiBasePath = "http://x/assets/fotos" }),
        new Env { ContentRootPath = "/ignored" }, cache);

    [Fact]
    public void ResolveRoot_UsesTheConfiguredPath_AbsoluteOrRelativeToContentRoot()
    {
        Assert.Equal("/srv/fotos", FotoLocator.ResolveRoot("/app", "/srv/fotos"));
        Assert.Equal("/app/fotos_cand", FotoLocator.ResolveRoot("/app", "./fotos_cand"));
        Assert.Equal("/app/fotos_cand", FotoLocator.ResolveRoot("/app", null));
    }

    [Fact]
    public void GetFotoUrl_IsNullWhenTheFileDoesNotExist()
    {
        Assert.Null(Create().GetFotoUrl(2022, "SP", "250001"));
    }

    [Fact]
    public void GetFotoUrl_ReturnsTheUrlWhenTheFileExists()
    {
        var dir = Path.Combine(root, "foto_cand2022_SP_div");
        Directory.CreateDirectory(dir);
        File.WriteAllBytes(Path.Combine(dir, "FSP250001_div.jpg"), new byte[] { 1 });

        Assert.Equal("http://x/assets/fotos/foto_cand2022_SP_div/FSP250001_div.jpg", Create().GetFotoUrl(2022, "SP", "250001"));
    }

    [Fact]
    public void GetFotoUrl_CachesTheExistsResult()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 10 });
        var locator = Create(cache);
        Assert.Null(locator.GetFotoUrl(2022, "SP", "1"));

        var dir = Path.Combine(root, "foto_cand2022_SP_div");
        Directory.CreateDirectory(dir);
        File.WriteAllBytes(Path.Combine(dir, "FSP1_div.jpg"), new byte[] { 1 });

        Assert.Null(locator.GetFotoUrl(2022, "SP", "1")); // still the cached "missing"
        Assert.Equal(1, cache.Count);
    }

    [Theory]
    [InlineData(null, "1")]
    [InlineData("SP", null)]
    [InlineData("SP", "../../etc/passwd")]
    public void GetFotoUrl_IsNullForMissingPartsOrPathTraversal(string? uf, string? sq)
    {
        Assert.Null(Create().GetFotoUrl(2022, uf, sq));
    }
}
