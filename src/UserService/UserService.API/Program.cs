using FluentValidation;
using MediatR;
using Serilog;
using UserService.Core.Features.Users.Commands;
using UserService.Core.DTOs;
using UserService.Core.Features.Users.Queries;
using UserService.Core.Validators;
using UserService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.File("logs/userservice-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "User Service API", 
        Version = "v1",
        Description = "A microservice for managing users in the CMS system",
        Contact = new() { Name = "CMS Team" }
    });
    
    // Include XML comments for better API documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateUserCommand).Assembly));

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();

// Add validation behavior
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UserService.Core.Behaviors.ValidationBehavior<,>));

// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service API V1");
        c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UserService.Infrastructure.Data.UserDbContext>();
    
    // Only call EnsureCreatedAsync for in-memory databases
    if (context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
    {
        await context.Database.EnsureCreatedAsync();
        // Log message already handled in DependencyInjection.cs
    }
    // PostgreSQL logging handled in DependencyInjection.cs
}

try
{
    Log.Information("Starting User Service API");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "User Service API terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program 
{ 
    protected Program() { }
}
