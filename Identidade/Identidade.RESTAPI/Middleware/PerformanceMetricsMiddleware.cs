using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Identidade.RESTAPI.Middleware
{
    public class PerformanceMetricsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMetricsMiddleware> _logger;

        public PerformanceMetricsMiddleware(RequestDelegate next, ILogger<PerformanceMetricsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestPath = context.Request.Path;
            var requestMethod = context.Request.Method;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                _logger.LogInformation(
                    "[BASELINE_METRIC] HTTP {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}",
                    requestMethod,
                    requestPath,
                    stopwatch.ElapsedMilliseconds,
                    context.Response.StatusCode
                );
            }
        }
    }
}