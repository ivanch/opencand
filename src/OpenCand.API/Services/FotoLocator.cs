using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OpenCand.API.Config;

namespace OpenCand.API.Services
{
    /// <summary>Resolves the photos folder from FotosSettings.Path and builds photo URLs only for files that exist.</summary>
    public class FotoLocator
    {
        private static readonly TimeSpan ExistsCacheExpiration = TimeSpan.FromHours(1);

        private readonly FotosSettings settings;
        private readonly IMemoryCache? cache;

        public string RootPath { get; }

        public FotoLocator(IOptions<FotosSettings> settings, IHostEnvironment environment, IMemoryCache? cache = null)
        {
            this.settings = settings.Value;
            this.cache = cache;
            RootPath = ResolveRoot(environment.ContentRootPath, this.settings.Path);
        }

        public static string ResolveRoot(string contentRoot, string? configuredPath)
            => Path.GetFullPath(Path.Combine(contentRoot, string.IsNullOrWhiteSpace(configuredPath) ? "fotos_cand" : configuredPath));

        internal static string RelativePath(int ano, string siglaUf, string sqCandidato)
            => $"foto_cand{ano}_{siglaUf}_div/F{siglaUf}{sqCandidato}_div.jpg";

        /// <summary>The public URL of the candidate photo, or null when the file is not on disk.</summary>
        public string? GetFotoUrl(int ano, string? siglaUf, string? sqCandidato)
        {
            if (string.IsNullOrEmpty(siglaUf) || string.IsNullOrEmpty(sqCandidato))
                return null;

            var relative = RelativePath(ano, siglaUf, sqCandidato);
            return Exists(relative) ? $"{settings.ApiBasePath}/{relative}" : null;
        }

        private bool Exists(string relativePath)
        {
            var fullPath = Path.GetFullPath(Path.Combine(RootPath, relativePath));
            // Values come from the database, but never let one escape the photos folder.
            if (!fullPath.StartsWith(RootPath, StringComparison.Ordinal))
                return false;

            if (cache == null)
                return File.Exists(fullPath);

            return cache.GetOrCreate("FotoExists_" + relativePath, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = ExistsCacheExpiration;
                entry.Size = 1;
                return File.Exists(fullPath);
            });
        }
    }
}
