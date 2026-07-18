using System.Diagnostics;

namespace GameStore.Api.Middleware;

/// <summary>
/// Middleware untuk mengukur execution time setiap HTTP request.
/// 
/// Cara kerja:
/// 1. Sebelum request diproses, start Stopwatch
/// 2. Setelah response selesai, stop Stopwatch
/// 3. Tambahkan header "X-Response-Time-Ms" ke response
/// 4. Log warning jika response > 1000ms (1 detik)
/// 
/// Cara membaca di client:
/// Response Headers akan berisi:
///   X-Response-Time-Ms: 42.15
/// 
/// Kenapa Stopwatch bukan DateTime?
/// - Stopwatch menggunakan high-resolution timer (QueryPerformanceCounter di Windows)
/// - Akurasi nanosecond vs DateTime yang hanya ~15ms resolution
/// - Untuk benchmarking, akurasi tinggi sangat penting
/// </summary>
public class RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
{
    private const long SlowRequestThresholdMs = 1000; // 1 detik

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // Register callback untuk menambahkan header sebelum response dikirim
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
