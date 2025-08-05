using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IdentityService.Core.Entities;
using IdentityService.Infrastructure.Data;
using IdentityService.Infrastructure.Repositories;
using IdentityService.Infrastructure.Repositories.Interfaces;
using IdentityService.Infrastructure.Services;
using IdentityService.Infrastructure.Services.Interfaces;
using Npgsql;

namespace IdentityService.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var useInMemory = ShouldUseInMemoryDatabase(connectionString);

        if (useInMemory)
        {
            Console.WriteLine("Using in-memory database for IdentityService");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("IdentityServiceDb"));
        }
        else
        {
            Console.WriteLine("Using PostgreSQL database for IdentityService");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString,
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        }

        services.AddIdentityCore<ApplicationUser>(options =>
        {

            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        services.AddScoped<IAuditService, AuditService>();

        services.AddScoped<IJwtService>(provider =>
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is required");
            var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer is required");
            var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience is required");
            var keyId = jwtSettings["KeyId"] ?? "cms-key-1";

            Console.WriteLine($"Identity Service JWT Settings - KeyId: {keyId}, Secret Length: {secret.Length}, Issuer: {issuer}, Audience: {audience}");

            return new JwtService(
                secret: secret,
                issuer: issuer,
                audience: audience,
                expirationMinutes: jwtSettings.GetValue<int>("ExpiryMinutes", 60),
                keyId: keyId
            );
        });

        return services;
    }

    private static bool ShouldUseInMemoryDatabase(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return true;

        try
        {
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return false;
        }
        catch
        {
            return true;
        }
    }
}
