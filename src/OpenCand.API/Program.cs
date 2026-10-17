using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using OpenCand.API.Config;
using OpenCand.API.Repository;
using OpenCand.API.Services;
using OpenCand.Repository;

namespace OpenCand.API
{
    public class Program
    {
        public static void Main(string[] args)
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

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            var workingDir = contentRoot;
            var fotosPath = Path.Combine(workingDir, "fotos_cand");
            if (!Directory.Exists(fotosPath))
            {
                Directory.CreateDirectory(fotosPath);
            }
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(fotosPath),
                RequestPath = "/assets/fotos"
            });
            app.UseHttpsRedirection();

            // Use rate limiting middleware
            app.UseRateLimiter();

            app.UseAuthorization();

            app.MapControllers();

            app.UseCors(x => x.AllowAnyMethod()
                              .AllowAnyHeader()
                              .SetIsOriginAllowed(origin => true) // allow any origin
                              .AllowCredentials()); // allow credentials


            app.Run();
        }

        private static void SetupServices(WebApplicationBuilder builder)
        {
            builder.Services.Configure<FotosSettings>(builder.Configuration.GetSection("FotosSettings"));

            // Configure memory cache with size limit from appsettings
            var sizeLimitMB = builder.Configuration.GetValue<int>("CacheSettings:SizeLimitMB", 15);
            builder.Services.AddMemoryCache(options =>
            {
                options.SizeLimit = sizeLimitMB * 1024L * 1024L; // Convert MB to bytes
            });

            builder.Services.AddScoped(provider => new OpenCandRepository(provider.GetRequiredService<IConfiguration>(), provider.GetService<IMemoryCache>()));
            builder.Services.AddScoped(provider => new CandidatoRepository(provider.GetRequiredService<IConfiguration>(), provider.GetService<IMemoryCache>()));
            builder.Services.AddScoped(provider => new BemCandidatoRepository(provider.GetRequiredService<IConfiguration>(), provider.GetService<IMemoryCache>()));
            builder.Services.AddScoped(provider => new DespesaReceitaRepository(provider.GetRequiredService<IConfiguration>(), provider.GetService<IMemoryCache>()));
            builder.Services.AddScoped(provider => new EstatisticaRepository(provider.GetRequiredService<IConfiguration>(), provider.GetService<IMemoryCache>()));

            builder.Services.AddScoped<OpenCandService>();
            builder.Services.AddScoped<EstatisticaService>();

            // Add cache preload background service
            builder.Services.AddHostedService<CachePreloadService>();
        }
    }
}
