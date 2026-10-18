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
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // Global safety net: one bucket per client IP (not per path).
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetClientIp(httpContext),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 2000,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));

                // Named policies are partitioned per client IP, so one client cannot exhaust the quota of others.
                AddPerClientPolicy(options, DefaultPolicy, permitLimit: 400, queueLimit: 100);
                AddPerClientPolicy(options, CandidatoSearchPolicy, permitLimit: 300, queueLimit: 200);
                AddPerClientPolicy(options, CpfRevealPolicy, permitLimit: 20, queueLimit: 5);

                options.OnRejected = async (context, token) =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger("RateLimitingConfig");
                    logger.LogWarning("Rate limit exceeded for client {ClientIp}", GetClientIp(context.HttpContext));

                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                    }

                    await context.HttpContext.Response.WriteAsync(
                        "Rate limit exceeded. Please try again later.", cancellationToken: token);
                };
            });
        }

        private static void AddPerClientPolicy(RateLimiterOptions options, string policyName, int permitLimit, int queueLimit)
        {
            options.AddPolicy(policyName, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIp(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = queueLimit
                    }));
        }

        // RemoteIpAddress is already the real client IP when UseForwardedHeaders is configured for the trusted proxy.
        private static string GetClientIp(HttpContext httpContext)
            => httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
