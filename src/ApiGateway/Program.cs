using Serilog;
using Serilog.Enrichers.CorrelationId;
using Serilog.Context;
using Yarp.ReverseProxy;
using AspNetCoreRateLimit;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog from appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithCorrelationId()
    .Enrich.FromLogContext()
    .CreateLogger();

// Add Serilog
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// Add HTTP Client with Polly for Circuit Breaker
builder.Services.AddHttpClient("resilient")
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

// Add Swagger for API documentation with microservices aggregation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "CMS API Gateway", 
        Version = "v1",
        Description = "Centralized API Gateway for CMS Microservices"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // Add microservices Swagger endpoints via proxy (to avoid CORS issues)
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CMS API Gateway v1");
        c.SwaggerEndpoint("/proxy/userservice/swagger/v1/swagger.json", "User Service v1");
        c.SwaggerEndpoint("/proxy/contentservice/swagger/v1/swagger.json", "Content Service v1");
        
        c.RoutePrefix = string.Empty; // Swagger UI at root
        c.DisplayRequestDuration();
        c.EnableTryItOutByDefault();
    });
}

app.UseCors();

// Add Rate Limiting
app.UseIpRateLimiting();

// Advanced logging middleware with request/response details
app.Use(async (context, next) =>
{
    var correlationId = context.TraceIdentifier;
    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        var startTime = DateTime.UtcNow;
        
        Log.Information("Request started: {Method} {Path} {QueryString}", 
            context.Request.Method, 
            context.Request.Path, 
            context.Request.QueryString);

        // Log request headers in development
        if (app.Environment.IsDevelopment())
        {
            foreach (var header in context.Request.Headers)
            {
                Log.Debug("Request Header: {HeaderName} = {HeaderValue}", header.Key, string.Join(", ", header.Value.ToArray()));
            }
        }

        await next();

        var duration = DateTime.UtcNow - startTime;
        Log.Information("Request completed: {Method} {Path} {StatusCode} in {Duration}ms", 
            context.Request.Method, 
            context.Request.Path, 
            context.Response.StatusCode, 
            duration.TotalMilliseconds);
    }
});

// Health check endpoint
app.MapHealthChecks("/health");

// Add a simple status endpoint
app.MapGet("/status", () => new { Status = "API Gateway is running", Timestamp = DateTime.UtcNow })
    .WithName("GetStatus")
    .WithOpenApi();

// Add API Gateway info endpoint
app.MapGet("/gateway/info", () => new { 
    Gateway = "CMS API Gateway",
    Version = "1.0.0",
    Features = new[] {
        "Rate Limiting",
        "Circuit Breaker", 
        "Request Logging",
        "Health Checks",
        "Swagger Aggregation"
    },
    Uptime = DateTime.UtcNow,
    Services = new {
        UserService = "http://localhost:5001",
        ContentService = "http://localhost:5002"
    }
})
.WithName("GetGatewayInfo")
.WithOpenApi();

// Configure YARP
app.MapReverseProxy();

// Add Swagger proxy endpoints to avoid CORS issues
app.MapGet("/proxy/userservice/swagger/v1/swagger.json", async (HttpClient httpClient, IConfiguration config) =>
{
    try
    {
        var userServiceUrl = config["ServiceUrls:UserService"] ?? "http://localhost:5001";
        var response = await httpClient.GetStringAsync($"{userServiceUrl}/swagger/v1/swagger.json");
        return Results.Content(response, "application/json");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to fetch UserService swagger");
        return Results.Problem("Failed to fetch UserService swagger");
    }
});

app.MapGet("/proxy/contentservice/swagger/v1/swagger.json", async (HttpClient httpClient, IConfiguration config) =>
{
    try
    {
        var contentServiceUrl = config["ServiceUrls:ContentService"] ?? "http://localhost:5002";
        var response = await httpClient.GetStringAsync($"{contentServiceUrl}/swagger/v1/swagger.json");
        return Results.Content(response, "application/json");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to fetch ContentService swagger");
        return Results.Problem("Failed to fetch ContentService swagger");
    }
});

Log.Information("Starting API Gateway on port 5000");

// Policy functions for Circuit Breaker and Retry
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return Policy
        .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .RetryAsync(3, onRetry: (outcome, retryNumber, context) =>
        {
            Log.Warning("Retry {RetryNumber} for {Context}", retryNumber, context.OperationKey);
        });
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return Policy
        .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (ex, duration) => Log.Error("Circuit breaker opened for {Duration}s", duration.TotalSeconds),
            onReset: () => Log.Information("Circuit breaker reset"));
}

await app.RunAsync();
