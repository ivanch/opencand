using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace OpenCand.API.Config
{
    public static class RateLimitingConfig
    {
        public const string DefaultPolicy = "DefaultPolicy";
        public const string CandidatoSearchPolicy = "CandidatoSearchPolicy";
        public const string CpfRevealPolicy = "CpfRevealPolicy";

        public static void ConfigureRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetClientIdentifier(httpContext),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 2000, // Global limit per minute
                            Window = TimeSpan.FromMinutes(1)
                        }));

                // Default policy: 200 requests per minute with burst of 100
                options.AddFixedWindowLimiter(policyName: DefaultPolicy, options =>
                {
                    options.PermitLimit = 400;
                    options.Window = TimeSpan.FromMinutes(1);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 100; // Burst capacity
                });

                options.AddFixedWindowLimiter(policyName: CandidatoSearchPolicy, options =>
                {
                    options.PermitLimit = 300;
                    options.Window = TimeSpan.FromMinutes(1);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 200; // Burst capacity
                });

                options.AddFixedWindowLimiter(policyName: CpfRevealPolicy, options =>
                {
                    options.PermitLimit = 20;
                    options.Window = TimeSpan.FromMinutes(1);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 5; // Burst capacity
                });

                options.OnRejected = async (context, token) =>
                {
                    var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger("RateLimitingConfig");
                    var clientIdentifier = GetClientIdentifier(context.HttpContext);
                    logger.LogWarning("Rate limit exceeded for client {ClientIdentifier}", clientIdentifier);

                    context.HttpContext.Response.StatusCode = 429;
                    
                    var retryAfter = GetRetryAfter(context);
                    if (retryAfter.HasValue)
                    {
                        context.HttpContext.Response.Headers.Append("Retry-After", retryAfter.Value.ToString());
                    }

                    await context.HttpContext.Response.WriteAsync(
                        "Rate limit exceeded. Please try again later.", cancellationToken: token);
                };
            });
        }

        private static string GetClientIdentifier(HttpContext httpContext)
        {
            // Get client IP address for partitioning
            var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() 
                ?? httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() 
                ?? "unknown";
            
            // Include the endpoint in the partition key for endpoint-specific limits
            var endpoint = httpContext.Request.Path.ToString();
            
            return $"{clientIp}:{endpoint}";
        }

        private static int? GetRetryAfter(OnRejectedContext context)
        {
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                return (int)retryAfter.TotalSeconds;
            }
            return null;
        }
    }
}
