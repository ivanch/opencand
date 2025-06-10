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
            var builder = WebApplication.CreateBuilder(args);            // Add services to the container.
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

            var workingDir = Directory.GetCurrentDirectory();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(workingDir, "fotos_cand")),
                RequestPath = "/assets/fotos"
            });            app.UseHttpsRedirection();

            // Use rate limiting middleware
            app.UseRateLimiter();

            app.UseAuthorization();

            app.MapControllers();

            app.UseCors(x => x.AllowAnyMethod()
                              .AllowAnyHeader()
                              .SetIsOriginAllowed(origin => true) // allow any origin
                              .AllowCredentials()); // allow credentials


            app.Run();
        }        private static void SetupServices(WebApplicationBuilder builder)
        {
            builder.Services.Configure<FotosSettings>(builder.Configuration.GetSection("FotosSettings"));
            builder.Services.AddMemoryCache();
            
            // Register repositories with IMemoryCache
            builder.Services.AddScoped<OpenCandRepository>(provider =>
                new OpenCandRepository(provider.GetRequiredService<IConfiguration>(), provider.GetService<IMemoryCache>()));
            builder.Services.AddScoped<CandidatoRepository>(provider =>
                new CandidatoRepository(provider.GetRequiredService<IConfiguration>(), provider.GetService<IMemoryCache>()));
            builder.Services.AddScoped<BemCandidatoRepository>(provider =>
                new BemCandidatoRepository(provider.GetRequiredService<IConfiguration>(), provider.GetService<IMemoryCache>()));
            builder.Services.AddScoped<DespesaReceitaRepository>(provider =>
                new DespesaReceitaRepository(provider.GetRequiredService<IConfiguration>(), provider.GetService<IMemoryCache>()));
            
            builder.Services.AddScoped<OpenCandService>();
        }
    }
}
