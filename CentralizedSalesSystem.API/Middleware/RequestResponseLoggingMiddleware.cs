using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Text.RegularExpressions;

namespace CentralizedSalesSystem.API.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly Serilog.ILogger Logger = Log.ForContext<RequestResponseLoggingMiddleware>();
    private static readonly Regex SensitiveDataRegex = new(
        @"""(password|passwordHash|token|secret|key)""\s*:\s*""[^""]*""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public RequestResponseLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];

        LogRequest(context, requestId);

        var originalResponseBody = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
            stopwatch.Stop();
            
            await LogResponseAsync(context, requestId, stopwatch.ElapsedMilliseconds, responseBody);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.Error(ex, "[{RequestId}] Exception: {Message} ({Duration}ms)", 
                requestId, ex.Message, stopwatch.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalResponseBody);
        }
    }

    private void LogRequest(HttpContext context, string requestId)
    {
        var request = context.Request;
        var user = context.User?.Identity?.Name ?? "Anonymous";
        
        Logger.Information("[{RequestId}] {Method} {Path}{QueryString} | User: {User} | ContentType: {ContentType}",
            requestId, request.Method, request.Path, request.QueryString, user, request.ContentType ?? "none");
    }

    private async Task LogResponseAsync(HttpContext context, string requestId, long durationMs, MemoryStream responseBody)
    {
        var response = context.Response;
        var statusCode = response.StatusCode;
        
        var logAction = statusCode switch
        {
            >= 500 => (Action<string, object[]>)Logger.Error,
            >= 400 => Logger.Warning,
            _ => Logger.Information
        };

        logAction("[{RequestId}] {StatusCode} | {Duration}ms | ContentType: {ContentType}",
            new object[] { requestId, statusCode, durationMs, response.ContentType ?? "none" });
    }
}
