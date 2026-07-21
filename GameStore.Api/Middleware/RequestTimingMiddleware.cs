using System.Diagnostics;

namespace GameStore.Api.Middleware;

public class RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
{
    private const long SlowRequestThresholdMs = 1000; // 1 detik

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        context.Response.OnStarting(() =>
        {
            stopwatch.Stop();
            double elapsedMs = stopwatch.Elapsed.TotalMilliseconds;

            context.Response.Headers["X-Response-Time-Ms"] = elapsedMs.ToString("F2");

            string method = context.Request.Method;
            string path = context.Request.Path;

            if (elapsedMs > SlowRequestThresholdMs)
            {
                logger.LogWarning(
                    "⚠️ SLOW REQUEST: {Method} {Path} took {ElapsedMs:F2}ms (threshold: {Threshold}ms)",
                    method, path, elapsedMs, SlowRequestThresholdMs);
            }
            else
            {
                logger.LogInformation(
                    "✅ {Method} {Path} completed in {ElapsedMs:F2}ms",
                    method, path, elapsedMs);
            }

            return Task.CompletedTask;
        });

        await next(context);
    }
}
