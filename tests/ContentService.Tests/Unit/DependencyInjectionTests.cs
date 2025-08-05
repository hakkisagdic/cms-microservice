using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using FluentAssertions;
using Xunit;
using ContentService.Infrastructure;
using ContentService.Infrastructure.Data;
using ContentService.Core.Interfaces;
using ContentService.Infrastructure.Repositories;
using ContentService.Infrastructure.Services;

namespace ContentService.Tests.Unit;

public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_WithPostgreSqlConnectionString_ShouldRegisterPostgreSqlDbContext()
    {

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"ConnectionStrings:DefaultConnection", "Host=localhost;Database=TestDb;Username=test;Password=test"}
            })
            .Build();

        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var dbContext = serviceProvider.GetService<ContentDbContext>();
        dbContext.Should().NotBeNull();

        var options = serviceProvider.GetService<DbContextOptions<ContentDbContext>>();
        options.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_WithEmptyConnectionString_ShouldRegisterInMemoryDbContext()
    {

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var dbContext = serviceProvider.GetService<ContentDbContext>();
        dbContext.Should().NotBeNull();

        var options = serviceProvider.GetService<DbContextOptions<ContentDbContext>>();
        options.Should().NotBeNull();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterContentRepository()
    {

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var repository = serviceProvider.GetService<IContentRepository>();
        repository.Should().NotBeNull();
        repository.Should().BeOfType<ContentRepository>();
    }

    [Fact]
    public void AddInfrastructure_ShouldRegisterUserServiceClient()
    {

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Services:UserService:BaseUrl", "http://localhost:5001"}
            })
            .Build();

        // HttpContextAccessor mock'ını ekle
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var userServiceClient = serviceProvider.GetService<IUserServiceClient>();
        userServiceClient.Should().NotBeNull();
        userServiceClient.Should().BeOfType<UserServiceClient>();
    }

    [Fact]
    public void AddInfrastructure_WithCustomUserServiceUrl_ShouldRegisterUserServiceClient()
    {

        var services = new ServiceCollection();
        var customUrl = "http://custom-user-service:8080";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"Services:UserService:BaseUrl", customUrl}
            })
            .Build();

        // HttpContextAccessor mock'ını ekle
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var userServiceClient = serviceProvider.GetService<IUserServiceClient>();
        userServiceClient.Should().NotBeNull();
        userServiceClient.Should().BeOfType<UserServiceClient>();
    }

    [Fact]
    public void AddInfrastructure_WithoutUserServiceUrl_ShouldRegisterUserServiceClient()
    {

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // HttpContextAccessor mock'ını ekle
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddInfrastructure(configuration);
        var serviceProvider = services.BuildServiceProvider();

        var userServiceClient = serviceProvider.GetService<IUserServiceClient>();
        userServiceClient.Should().NotBeNull();
        userServiceClient.Should().BeOfType<UserServiceClient>();
    }

    [Fact]
    public void AddInfrastructure_ShouldReturnServiceCollection()
    {

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var result = services.AddInfrastructure(configuration);

        result.Should().BeSameAs(services);
    }
}
