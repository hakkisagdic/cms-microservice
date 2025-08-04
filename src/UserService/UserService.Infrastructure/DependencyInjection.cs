using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Repositories;

namespace UserService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database configuration - test PostgreSQL connection first
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var useInMemory = ShouldUseInMemoryDatabase(connectionString);
        
        if (useInMemory)
        {
            Console.WriteLine("Using in-memory database for UserService");
            services.AddDbContext<UserDbContext>(options =>
                options.UseInMemoryDatabase("UserServiceDb"));
        }
        else
        {
            Console.WriteLine("Using PostgreSQL database for UserService");
            services.AddDbContext<UserDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        // Repository registration
        services.AddScoped<IUserRepository, UserRepository>();

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
