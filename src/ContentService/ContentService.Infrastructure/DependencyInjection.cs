using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ContentService.Core.Interfaces;
using ContentService.Infrastructure.Data;
using ContentService.Infrastructure.Repositories;
using ContentService.Infrastructure.Services;

namespace ContentService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database configuration - test PostgreSQL connection first
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var useInMemory = ShouldUseInMemoryDatabase(connectionString);
        
        if (useInMemory)
        {
            Console.WriteLine("Using in-memory database for ContentService");
            services.AddDbContext<ContentDbContext>(options =>
                options.UseInMemoryDatabase("ContentServiceDb"));
        }
        else
        {
            Console.WriteLine("Using PostgreSQL database for ContentService");
            services.AddDbContext<ContentDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        // Repository registration
        services.AddScoped<IContentRepository, ContentRepository>();
        
        // External service clients
        services.AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
        {
            var userServiceUrl = configuration["Services:UserService:BaseUrl"] ?? "http://localhost:5001";
            client.BaseAddress = new Uri(userServiceUrl);
        });

        return services;
    }

    private static bool ShouldUseInMemoryDatabase(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return true;

        try
        {
            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            connection.Open();
            return false; // PostgreSQL connection successful
        }
        catch (Exception)
        {
            // Suppress the exception to avoid log noise
            return true; // Use in-memory if PostgreSQL connection fails
        }
    }
}
