using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CMS.Common.Configuration;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddCommonConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        // Add strongly typed configuration
        services.Configure<DatabaseOptions>(configuration.GetSection("Database"));
        services.Configure<JwtOptions>(configuration.GetSection("JwtSettings"));
        services.Configure<RedisOptions>(configuration.GetSection("Redis"));
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
        services.Configure<OpenTelemetryOptions>(configuration.GetSection("OpenTelemetry"));
        
        return services;
    }
}

public class DatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 30;
    public int MaxRetryCount { get; set; } = 3;
    public bool EnableSensitiveDataLogging { get; set; } = false;
}

public class JwtOptions
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
    public string KeyId { get; set; } = string.Empty;
}

public class RedisOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public int Database { get; set; } = 0;
    public string InstanceName { get; set; } = "CMS_";
    public TimeSpan DefaultExpiry { get; set; } = TimeSpan.FromHours(1);
}

public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "cms_events";
}

public class OpenTelemetryOptions
{
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceVersion { get; set; } = "1.0.0";
    public string OtlpEndpoint { get; set; } = string.Empty;
    public bool EnableConsoleExporter { get; set; } = false;
}
