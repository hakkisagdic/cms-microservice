using Serilog;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Constants
const string ApplicationJson = "application/json";

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/apigateway-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks();

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is required");
var keyId = jwtSettings["KeyId"] ?? "cms-key-1";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(secretKey)) { KeyId = keyId },
            ClockSkew = TimeSpan.Zero
        };
        
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("JWT Authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Information("JWT Token validated for user: {UserId}", 
                    context.Principal?.FindFirst("sub")?.Value ?? "Unknown");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "CMS API Gateway", 
        Version = "v1",
        Description = "Centralized API Gateway for CMS Microservices"
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    
    // Remove server header for security
    context.Response.Headers.Remove("Server");
    
    await next();
});

// Global exception handling
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = ApplicationJson;
        
        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;
        
        Log.Error(exception, "Unhandled exception occurred");
        
        var response = new
        {
            Error = "An internal server error occurred",
            Time = DateTime.UtcNow,
            TraceId = context.TraceIdentifier
        };
        
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    });
});

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CMS API Gateway v1");
        c.SwaggerEndpoint("/proxy/identity/swagger.json", "Identity Service v1");
        c.SwaggerEndpoint("/proxy/users/swagger.json", "User Service v1");
        c.SwaggerEndpoint("/proxy/contents/swagger.json", "Content Service v1");
        
        c.RoutePrefix = string.Empty;
        c.DisplayRequestDuration();
        c.EnableTryItOutByDefault();
    });
}

app.UseCors();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Simple request logging
app.Use(async (context, next) =>
{
    var startTime = DateTime.UtcNow;
    Log.Information("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
    
    await next();
    
    var duration = DateTime.UtcNow - startTime;
    Log.Information("Response: {StatusCode} in {Duration}ms", context.Response.StatusCode, duration.TotalMilliseconds);
});

app.MapHealthChecks("/health");

app.MapGet("/status", () => new { Status = "Running", Time = DateTime.UtcNow })
    .WithName("GetStatus")
    .WithOpenApi();

// JWT validation test endpoint (only in development)
if (app.Environment.IsDevelopment())
{
    app.MapGet("/jwt-test", (HttpContext context) =>
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        
        if (authHeader == null || !authHeader.StartsWith("Bearer "))
        {
            return Results.BadRequest(new { Error = "No Bearer token provided", Time = DateTime.UtcNow });
        }
        
        var token = authHeader.Substring("Bearer ".Length);
        
        return Results.Ok(new 
        { 
            Message = "Token received successfully", 
            TokenLength = token.Length,
            TokenPreview = token.Substring(0, Math.Min(50, token.Length)) + "...",
            Time = DateTime.UtcNow
        });
    })
    .WithName("JwtTest")
    .WithOpenApi()
    .WithTags("Development");
}

// Protected endpoint for testing JWT
app.MapGet("/protected", (HttpContext context) =>
{
    var user = context.User;
    var userId = user.FindFirst("sub")?.Value ?? user.FindFirst("id")?.Value;
    var email = user.FindFirst("email")?.Value;
    
    return Results.Ok(new 
    { 
        Message = "Access granted! This is a protected endpoint.",
        UserId = userId,
        Email = email,
        Claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList(),
        Time = DateTime.UtcNow
    });
})
.RequireAuthorization()
.WithName("GetProtected")
.WithOpenApi();

// Swagger proxy endpoints
app.MapGet("/proxy/identity/swagger.json", async (HttpClient httpClient, IConfiguration configuration) =>
{
    try
    {
        var identityServiceUrl = configuration["ServiceUrls:IdentityService"];
        var response = await httpClient.GetStringAsync($"{identityServiceUrl}/swagger/v1/swagger.json");
        return Results.Content(response, "application/json");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to fetch Identity Service swagger");
        return Results.Problem("Identity Service not available");
    }
})
.ExcludeFromDescription();

app.MapGet("/proxy/users/swagger.json", async (HttpClient httpClient, IConfiguration configuration) =>
{
    try
    {
        var userServiceUrl = configuration["ServiceUrls:UserService"];
        var response = await httpClient.GetStringAsync($"{userServiceUrl}/swagger/v1/swagger.json");
        return Results.Content(response, "application/json");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to fetch User Service swagger");
        return Results.Problem("User Service not available");
    }
})
.ExcludeFromDescription();

app.MapGet("/proxy/contents/swagger.json", async (HttpClient httpClient, IConfiguration configuration) =>
{
    try
    {
        var contentServiceUrl = configuration["ServiceUrls:ContentService"];
        var response = await httpClient.GetStringAsync($"{contentServiceUrl}/swagger/v1/swagger.json");
        return Results.Content(response, "application/json");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to fetch Content Service swagger");
        return Results.Problem("Content Service not available");
    }
})
.ExcludeFromDescription();

// Configure YARP reverse proxy
app.MapReverseProxy();

Log.Information("Starting CMS API Gateway...");

await app.RunAsync();
