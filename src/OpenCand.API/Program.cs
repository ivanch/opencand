using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using OpenCand.API.Health;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Npgsql;
using OpenCand.API.Config;
using OpenCand.API.Repository;
using OpenCand.API.Services;
using OpenCand.Core.Database;
using OpenCand.Repository;

namespace OpenCand.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var contentRoot = File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))
                ? Directory.GetCurrentDirectory()
                : AppContext.BaseDirectory;

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = args,
                ContentRootPath = contentRoot
            });            // Add services to the container.
            builder.Services.AddControllers();

            SetupServices(builder);

            // Configure rate limiting
            builder.Services.ConfigureRateLimiting();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Bring the schema up to date before serving; a failure here stops the process instead of serving a stale schema.
            if (app.Configuration.GetValue("Database:AutoMigrate", true))
            {
                await using var dataSource = NpgsqlDataSource.Create(app.Configuration["DatabaseSettings:ConnectionString"]
                    ?? throw new InvalidOperationException("DatabaseSettings:ConnectionString is not configured."));
                await DbMigrator.MigrateAsync(dataSource, app.Services.GetRequiredService<ILogger<Program>>());
            }

            // Must run first so the rate limiter and logs see the real client IP.
            app.UseForwardedHeaders();

            app.UseResponseCompression();

            app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
            {
                var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                var (status, title) = error switch
                {
                    ArgumentException => (StatusCodes.Status400BadRequest, "Bad request"),
                    KeyNotFoundException => (StatusCodes.Status404NotFound, "Not found"),
                    _ => (StatusCodes.Status500InternalServerError, "Internal server error")
                };

                context.Response.StatusCode = status;
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = status,
                    Title = title,
                    Detail = status < 500 ? error?.Message : null
                });
            }));

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            var fotosPath = app.Services.GetRequiredService<FotoLocator>().RootPath;
            if (!Directory.Exists(fotosPath))
            {
                Directory.CreateDirectory(fotosPath);
            }
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(fotosPath),
                RequestPath = "/assets/fotos"
            });

            if (app.Configuration.GetValue<bool>("Https:Redirect"))
            {
                app.UseHttpsRedirection();
            }

            // CORS before the rate limiter so 429 responses still carry CORS headers.
            app.UseCors();

            app.UseRateLimiter();

            app.UseAuthorization();

            app.MapControllers();

            app.MapHealthChecks("/health").DisableRateLimiting();

            await app.RunAsync();
        }

        private static void SetupServices(WebApplicationBuilder builder)
        {
            ConfigureForwardedHeaders(builder);
            ConfigureCors(builder);

            builder.Services.Configure<FotosSettings>(builder.Configuration.GetSection("FotosSettings"));

            // Cache is sized by entry count: every entry is added with Size = 1 (see BaseRepository).
            var maxCacheEntries = builder.Configuration.GetValue<int>("CacheSettings:MaxEntries", 5000);
            builder.Services.AddMemoryCache(options =>
            {
                options.SizeLimit = maxCacheEntries;
            });

            builder.Services.AddSingleton<FotoLocator>();
            builder.Services.AddSingleton<PopularityTracker>();

            builder.Services.AddScoped(provider => new OpenCandRepository(provider.GetRequiredService<IConfiguration>(), provider.GetRequiredService<FotoLocator>().RootPath, provider.GetService<IMemoryCache>()));
            builder.Services.AddScoped(provider => new CandidatoRepository(provider.GetRequiredService<IConfiguration>(), provider.GetService<IMemoryCache>()));
            builder.Services.AddScoped(provider => new BemCandidatoRepository(provider.GetRequiredService<IConfiguration>(), provider.GetService<IMemoryCache>()));
            builder.Services.AddScoped(provider => new DespesaReceitaRepository(provider.GetRequiredService<IConfiguration>(), provider.GetService<IMemoryCache>()));
            builder.Services.AddScoped(provider => new EstatisticaRepository(provider.GetRequiredService<IConfiguration>(), provider.GetService<IMemoryCache>()));

            builder.Services.AddScoped<OpenCandService>();
            builder.Services.AddScoped<EstatisticaService>();

            builder.Services.AddResponseCompression();
            builder.Services.AddHealthChecks().AddCheck<NpgsqlHealthCheck>("postgres");

            // Cache preload (also re-run by DataVersionWatcher), cache invalidation after ETL runs, and popularity flushing
            builder.Services.AddSingleton<CachePreloadService>();
            builder.Services.AddHostedService(provider => provider.GetRequiredService<CachePreloadService>());
            builder.Services.AddHostedService<DataVersionWatcher>();
            builder.Services.AddHostedService<PopularityFlushService>();
        }

        private static void ConfigureForwardedHeaders(WebApplicationBuilder builder)
        {
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                var proxies = builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? Array.Empty<string>();
                var networks = builder.Configuration.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]>() ?? Array.Empty<string>();

                if (proxies.Length > 0 || networks.Length > 0)
                {
                    options.KnownProxies.Clear();
                    options.KnownNetworks.Clear();
                }

                foreach (var proxy in proxies)
                {
                    options.KnownProxies.Add(IPAddress.Parse(proxy));
                }

                foreach (var network in networks)
                {
                    var parts = network.Split('/');
                    options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(
                        IPAddress.Parse(parts[0]), int.Parse(parts[1])));
                }
            });
        }

        private static void ConfigureCors(WebApplicationBuilder builder)
        {
            var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyMethod().AllowAnyHeader();

                    // No cookies/credentials are used by the API, so credentials are never allowed.
                    if (origins.Length > 0)
                        policy.WithOrigins(origins);
                    else
                        policy.AllowAnyOrigin();
                });
            });
        }
    }
}
