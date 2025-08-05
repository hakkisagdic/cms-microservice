using System.Diagnostics;

namespace CMS.Common.Tracing;

public static class ActivitySourceProvider
{
    public static readonly ActivitySource ApiGateway = new("CMS.ApiGateway");
    public static readonly ActivitySource UserService = new("CMS.UserService");
    public static readonly ActivitySource ContentService = new("CMS.ContentService");  
    public static readonly ActivitySource IdentityService = new("CMS.IdentityService");
}

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        
        // Add to response headers
        context.Response.Headers.TryAdd(CorrelationIdHeaderName, correlationId);
        
        // Add to logging context
        using var scope = context.RequestServices.GetRequiredService<ILogger<CorrelationIdMiddleware>>()
            .BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId });
            
        await _next(context);
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId))
        {
            return correlationId.FirstOrDefault() ?? Guid.NewGuid().ToString();
        }

        return Guid.NewGuid().ToString();
    }
}
