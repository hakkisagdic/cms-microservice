# Testing Kılavuzu

Bu dokümant, CMS Mikroservis projesinin test stratejisi, test yazma standartları ve best practice'lerini içerir.

## 📋 İçindekiler
1. [Test Stratejisi](#test-stratejisi)
2. [Test Pyramid](#test-pyramid)
3. [Unit Tests](#unit-tests)
4. [Integration Tests](#integration-tests)
5. [API Tests](#api-tests)
6. [Test Configuration](#test-configuration)
7. [Mocking Strategy](#mocking-strategy)
8. [Test Data Management](#test-data-management)
9. [Performance Tests](#performance-tests)
10. [Test Automation](#test-automation)

## 🎯 Test Stratejisi

### Test Felsefeleri
- **Test-Driven Development (TDD)**: Red-Green-Refactor cycle
- **Behavior-Driven Development (BDD)**: Given-When-Then scenarios
- **Fail Fast**: Erken hata tespiti için comprehensive test coverage
- **Continuous Testing**: CI/CD pipeline'da otomatik test execution

### Test Coverage Hedefleri
- **Unit Tests**: 90%+ code coverage
- **Integration Tests**: Critical business flows
- **API Tests**: Tüm endpoints ve edge cases
- **Performance Tests**: Load ve stress testing

## 🔺 Test Pyramid

```
                    /\
                   /  \
                  /    \
                 /  E2E  \     <- Az sayıda, UI flows
                /________\
               /          \
              /            \
             /  Integration  \   <- Orta sayıda, service interactions
            /________________\
           /                  \
          /                    \
         /      Unit Tests       \  <- Çok sayıda, business logic
        /________________________\
```

### Test Dağılımı
- **70% Unit Tests**: Fast, isolated, business logic
- **20% Integration Tests**: Database, external services
- **10% End-to-End Tests**: Complete user workflows

## 🧪 Unit Tests

### Test Project Yapısı

```
tests/
├── IdentityService.Tests/
│   ├── Application/
│   │   ├── Services/
│   │   │   ├── AuthenticationServiceTests.cs
│   │   │   └── UserServiceTests.cs
│   │   └── Validators/
│   │       ├── LoginRequestValidatorTests.cs
│   │       └── RegisterRequestValidatorTests.cs
│   ├── Infrastructure/
│   │   ├── Services/
│   │   │   ├── JwtServiceTests.cs
│   │   │   └── AuditServiceTests.cs
│   │   └── Repositories/
│   │       └── RefreshTokenRepositoryTests.cs
│   └── API/
│       └── Controllers/
│           └── AuthControllerTests.cs
├── UserService.Tests/
│   ├── Core/
│   │   ├── Commands/
│   │   │   └── CreateUserCommandHandlerTests.cs
│   │   ├── Queries/
│   │   │   └── GetUserByIdQueryHandlerTests.cs
│   │   └── Validators/
│   │       └── CreateUserCommandValidatorTests.cs
│   ├── Infrastructure/
│   │   └── Repositories/
│   │       └── UserRepositoryTests.cs
│   └── API/
│       └── Controllers/
│           └── UsersControllerTests.cs
└── ContentService.Tests/
    ├── Core/
    ├── Infrastructure/
    └── API/
```

### Sample Unit Tests

#### Identity Service - Authentication Service Test

```csharp
// IdentityService.Tests/Application/Services/AuthenticationServiceTests.cs
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using IdentityService.Application.Services;
using IdentityService.Core.DTOs.Authentication;
using IdentityService.Core.Entities;
using IdentityService.Infrastructure.Services.Interfaces;
using IdentityService.Infrastructure.Repositories.Interfaces;
using Xunit;

namespace IdentityService.Tests.Application.Services
{
    public class AuthenticationServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
        private readonly Mock<IAuditService> _auditServiceMock;
        private readonly AuthenticationService _service;

        public AuthenticationServiceTests()
        {
            _userManagerMock = MockUserManager<ApplicationUser>();
            _jwtServiceMock = new Mock<IJwtService>();
            _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
            _auditServiceMock = new Mock<IAuditService>();
            
            _service = new AuthenticationService(
                _userManagerMock.Object,
                _jwtServiceMock.Object,
                _refreshTokenRepositoryMock.Object,
                _auditServiceMock.Object);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccessResult()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "SecurePassword123!"
            };

            var user = new ApplicationUser
            {
                Id = "user-123",
                Email = request.Email,
                FirstName = "John",
                LastName = "Doe",
                Status = UserStatus.Active
            };

            var expectedToken = "jwt-token-here";
            var expectedRefreshToken = "refresh-token-here";

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, request.Password))
                .ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });
            _jwtServiceMock.Setup(x => x.GenerateJwtToken(It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IList<string>>()))
                .Returns(expectedToken);
            _jwtServiceMock.Setup(x => x.GenerateRefreshToken())
                .Returns(expectedRefreshToken);
            _jwtServiceMock.Setup(x => x.GetJwtIdFromToken(expectedToken))
                .Returns("jwt-id");

            // Act
            var result = await _service.LoginAsync(request, "127.0.0.1", "TestAgent");

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.AccessToken.Should().Be(expectedToken);
            result.RefreshToken.Should().Be(expectedRefreshToken);
            result.User.Should().NotBeNull();
            result.User.Email.Should().Be(request.Email);

            _auditServiceMock.Verify(x => x.LogLoginAttemptAsync(
                user.Id, user.Email, true, "127.0.0.1", "TestAgent", null), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ShouldReturnFailureResult()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "WrongPassword"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _service.LoginAsync(request, "127.0.0.1", "TestAgent");

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain("Invalid email or password");
            result.AccessToken.Should().BeNull();
            result.RefreshToken.Should().BeNull();
        }

        private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            return new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
        }
    }
}
```

#### Identity Service - JWT Service Test

```csharp
// IdentityService.Tests/Infrastructure/Services/JwtServiceTests.cs
using FluentAssertions;
using IdentityService.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace IdentityService.Tests.Infrastructure.Services
{
    public class JwtServiceTests
    {
        private readonly JwtService _jwtService;
        private const string TestSecret = "ThisIsATestSecretKeyThatIsAtLeast32Characters";
        private const string TestIssuer = "TestIssuer";
        private const string TestAudience = "TestAudience";
        private const string TestKeyId = "test-key-1";

        public JwtServiceTests()
        {
            _jwtService = new JwtService(TestSecret, TestIssuer, TestAudience, 60, TestKeyId);
        }

        [Fact]
        public void GenerateJwtToken_ShouldReturnValidToken()
        {
            // Arrange
            var userId = "user-123";
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";
            var roles = new List<string> { "User", "Admin" };

            // Act
            var token = _jwtService.GenerateJwtToken(userId, email, firstName, lastName, roles);

            // Assert
            token.Should().NotBeNullOrEmpty();

            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);

            jsonToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value.Should().Be(userId);
            jsonToken.Claims.FirstOrDefault(x => x.Type == "email")?.Value.Should().Be(email);
            jsonToken.Issuer.Should().Be(TestIssuer);
            jsonToken.Audiences.Should().Contain(TestAudience);
            jsonToken.Header.Kid.Should().Be(TestKeyId);
        }

        [Fact] 
        public void GenerateRefreshToken_ShouldReturnUniqueTokens()
        {
            // Act
            var token1 = _jwtService.GenerateRefreshToken();
            var token2 = _jwtService.GenerateRefreshToken();

            // Assert
            token1.Should().NotBeNullOrEmpty();
            token2.Should().NotBeNullOrEmpty();
            token1.Should().NotBe(token2);
        }

        [Fact]
        public void IsTokenExpired_WithExpiredToken_ShouldReturnTrue()
        {
            // Arrange
            var expiredJwtService = new JwtService(TestSecret, TestIssuer, TestAudience, -1, TestKeyId);
            var token = expiredJwtService.GenerateJwtToken("user-123", "test@example.com", "John", "Doe", new List<string>());

            // Act
            var isExpired = _jwtService.IsTokenExpired(token);

            // Assert
            isExpired.Should().BeTrue();
        }
    }
}
```

#### Command Handler Test

```csharp
// UserService.Tests/Core/Commands/CreateUserCommandHandlerTests.cs
using FluentAssertions;
using Moq;
using UserService.Core.Commands;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using Xunit;

namespace UserService.Tests.Core.Commands
{
    public class CreateUserCommandHandlerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly CreateUserCommandHandler _handler;

        public CreateUserCommandHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _handler = new CreateUserCommandHandler(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_WithValidCommand_ShouldCreateUser()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };

            _userRepositoryMock
                .Setup(x => x.ExistsAsync(It.Is<string>(email => email == command.Email)))
                .ReturnsAsync(false);

            _userRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync((User user) => 
                {
                    user.Id = Guid.NewGuid();
                    return user;
                });

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Email.Should().Be(command.Email);
            result.Data.FirstName.Should().Be(command.FirstName);
            result.Data.LastName.Should().Be(command.LastName);
            result.Data.Status.Should().Be(UserStatus.Active);

            _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithExistingEmail_ShouldReturnFailure()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "existing@example.com"
            };

            _userRepositoryMock
                .Setup(x => x.ExistsAsync(command.Email))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().Be("User with this email already exists");

            _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
        }

        [Theory]
        [InlineData("", "Doe", "john@example.com")]
        [InlineData("John", "", "john@example.com")]
        [InlineData("John", "Doe", "")]
        [InlineData("John", "Doe", "invalid-email")]
        public async Task Handle_WithInvalidData_ShouldReturnValidationError(
            string firstName, string lastName, string email)
        {
            // Arrange
            var command = new CreateUserCommand
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNullOrEmpty();
        }
    }
}
```

#### Repository Test

```csharp
// UserService.Tests/Infrastructure/Repositories/UserRepositoryTests.cs
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UserService.Core.Entities;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Repositories;
using Xunit;

namespace UserService.Tests.Infrastructure.Repositories
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly UserDbContext _context;
        private readonly UserRepository _repository;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new UserDbContext(options);
            _repository = new UserRepository(_context);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddUserToDatabase()
        {
            // Arrange
            var user = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Status = UserStatus.Active
            };

            // Act
            var result = await _repository.CreateAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().NotBe(Guid.Empty);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

            var savedUser = await _context.Users.FindAsync(result.Id);
            savedUser.Should().NotBeNull();
            savedUser.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ShouldReturnUser()
        {
            // Arrange
            var user = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Status = UserStatus.Active
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(user.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(user.Id);
            result.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var result = await _repository.GetByIdAsync(nonExistingId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task ExistsAsync_WithExistingEmail_ShouldReturnTrue()
        {
            // Arrange
            var user = new User
            {
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob.johnson@example.com",
                Status = UserStatus.Active
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.ExistsAsync(user.Email);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingEmail_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsAsync("nonexisting@example.com");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyUser()
        {
            // Arrange
            var user = new User
            {
                FirstName = "Alice",
                LastName = "Brown",
                Email = "alice.brown@example.com",
                Status = UserStatus.Active
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            user.FirstName = "Alicia";
            user.Status = UserStatus.Inactive;
            var result = await _repository.UpdateAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().Be("Alicia");
            result.Status.Should().Be(UserStatus.Inactive);
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
```

#### Controller Test

```csharp
// UserService.Tests/API/Controllers/UsersControllerTests.cs
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.API.Controllers;
using UserService.Core.Commands;
using UserService.Core.Common;
using UserService.Core.DTOs;
using UserService.Core.Queries;
using Xunit;

namespace UserService.Tests.API.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<UsersController>> _loggerMock;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<UsersController>>();
            _controller = new UsersController(_mediatorMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateUser_WithValidRequest_ShouldReturnCreated()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };

            var userDto = new UserDto
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Status = "Active"
            };

            var result = Result<UserDto>.Success(userDto);

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var response = await _controller.CreateUser(request);

            // Assert
            var createdResult = response.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.StatusCode.Should().Be(201);
            createdResult.Value.Should().BeEquivalentTo(userDto);
            createdResult.ActionName.Should().Be(nameof(UsersController.GetUser));
            createdResult.RouteValues["id"].Should().Be(userDto.Id);
        }

        [Fact]
        public async Task CreateUser_WithExistingEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "existing@example.com"
            };

            var result = Result<UserDto>.Failure("User with this email already exists");

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var response = await _controller.CreateUser(request);

            // Assert
            var badRequestResult = response.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("User with this email already exists");
        }

        [Fact]
        public async Task GetUser_WithExistingId_ShouldReturnOk()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userDto = new UserDto
            {
                Id = userId,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Status = "Active"
            };

            var result = Result<UserDto>.Success(userDto);

            _mediatorMock
                .Setup(x => x.Send(It.Is<GetUserByIdQuery>(q => q.Id == userId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var response = await _controller.GetUser(userId);

            // Assert
            var okResult = response.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(userDto);
        }

        [Fact]
        public async Task GetUser_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var result = Result<UserDto>.Failure("User not found");

            _mediatorMock
                .Setup(x => x.Send(It.Is<GetUserByIdQuery>(q => q.Id == userId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var response = await _controller.GetUser(userId);

            // Assert
            var notFoundResult = response.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().Be("User not found");
        }
    }
}
```

## 🔗 Integration Tests

### Test Configuration

```csharp
// tests/UserService.IntegrationTests/TestFixture.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UserService.Infrastructure.Data;

namespace UserService.IntegrationTests
{
    public class TestFixture : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                services.RemoveAll(typeof(DbContextOptions<UserDbContext>));
                services.RemoveAll(typeof(UserDbContext));

                // Add a database context using an in-memory database for testing
                services.AddDbContext<UserDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                });

                // Build the service provider
                var serviceProvider = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context
                using var scope = serviceProvider.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<UserDbContext>();

                // Ensure the database is created
                db.Database.EnsureCreated();
            });

            builder.UseEnvironment("Testing");
        }
    }
}
```

### Integration Test Example

```csharp
// tests/UserService.IntegrationTests/UsersControllerIntegrationTests.cs
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using UserService.Core.DTOs;
using UserService.Core.Entities;
using UserService.Infrastructure.Data;
using Xunit;

namespace UserService.IntegrationTests
{
    public class UsersControllerIntegrationTests : IClassFixture<TestFixture>
    {
        private readonly HttpClient _client;
        private readonly TestFixture _factory;

        public UsersControllerIntegrationTests(TestFixture factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateUser_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/users", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
            userDto.Should().NotBeNull();
            userDto.FirstName.Should().Be(request.FirstName);
            userDto.LastName.Should().Be(request.LastName);
            userDto.Email.Should().Be(request.Email);
            userDto.Status.Should().Be("Active");

            // Verify in database
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            savedUser.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateUser_WithDuplicateEmail_ShouldReturnBadRequest()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            
            var existingUser = new User
            {
                FirstName = "Existing",
                LastName = "User",
                Email = "existing@example.com",
                Status = UserStatus.Active
            };
            
            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            var request = new CreateUserRequest
            {
                FirstName = "New",
                LastName = "User",
                Email = "existing@example.com"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/users", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetUser_WithExistingId_ShouldReturnOk()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            
            var user = new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Status = UserStatus.Active
            };
            
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/users/{user.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
            userDto.Should().NotBeNull();
            userDto.Id.Should().Be(user.Id);
            userDto.Email.Should().Be(user.Email);
        }

        [Fact]
        public async Task GetUser_WithNonExistingId_ShouldReturnNotFound()
        {
            // Arrange
            var nonExistingId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/users/{nonExistingId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetUsers_ShouldReturnPagedResult()
        {
            // Arrange - Seed test data
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            
            var users = new List<User>
            {
                new() { FirstName = "User1", LastName = "Test", Email = "user1@test.com", Status = UserStatus.Active },
                new() { FirstName = "User2", LastName = "Test", Email = "user2@test.com", Status = UserStatus.Active },
                new() { FirstName = "User3", LastName = "Test", Email = "user3@test.com", Status = UserStatus.Inactive }
            };
            
            context.Users.AddRange(users);
            await context.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/users?page=1&pageSize=2");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<PagedResult<UserDto>>();
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().BeGreaterOrEqualTo(3);
            result.CurrentPage.Should().Be(1);
            result.PageSize.Should().Be(2);
        }
    }
}
```

## 🌐 API Tests

### Postman Collection Structure

```json
{
  "info": {
    "name": "CMS Microservices API Tests",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "variable": [
    {
      "key": "userServiceUrl",
      "value": "http://localhost:5001"
    },
    {
      "key": "contentServiceUrl",
      "value": "http://localhost:5002"
    }
  ],
  "item": [
    {
      "name": "User Service",
      "item": [
        {
          "name": "Create User - Success",
          "request": {
            "method": "POST",
            "header": [
              {
                "key": "Content-Type",
                "value": "application/json"
              }
            ],
            "body": {
              "mode": "raw",
              "raw": "{\n  \"firstName\": \"John\",\n  \"lastName\": \"Doe\",\n  \"email\": \"{{$randomEmail}}\"\n}"
            },
            "url": {
              "raw": "{{userServiceUrl}}/api/users",
              "host": ["{{userServiceUrl}}"],
              "path": ["api", "users"]
            }
          },
          "event": [
            {
              "listen": "test",
              "script": {
                "exec": [
                  "pm.test('Status code is 201', function () {",
                  "    pm.response.to.have.status(201);",
                  "});",
                  "",
                  "pm.test('Response has user data', function () {",
                  "    const jsonData = pm.response.json();",
                  "    pm.expect(jsonData).to.have.property('id');",
                  "    pm.expect(jsonData).to.have.property('firstName');",
                  "    pm.expect(jsonData).to.have.property('lastName');",
                  "    pm.expect(jsonData).to.have.property('email');",
                  "    pm.expect(jsonData.status).to.eql('Active');",
                  "});",
                  "",
                  "pm.test('Save user ID for subsequent tests', function () {",
                  "    const jsonData = pm.response.json();",
                  "    pm.collectionVariables.set('createdUserId', jsonData.id);",
                  "});"
                ]
              }
            }
          ]
        }
      ]
    }
  ]
}
```

### Newman (CLI) Test Runner

```bash
# Install Newman
npm install -g newman

# Run collection
newman run cms-microservices-tests.json \
  --environment cms-environment.json \
  --reporters cli,html \
  --reporter-html-export test-results.html

# Run with data file
newman run cms-microservices-tests.json \
  --data test-data.csv \
  --iteration-count 10
```

## ⚙️ Test Configuration

### Test Settings

```json
// tests/UserService.Tests/appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=UserServiceTestDb;Username=postgres;Password=postgres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

### Global Test Settings

```csharp
// tests/GlobalUsings.cs
global using Xunit;
global using FluentAssertions;
global using Moq;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.EntityFrameworkCore;
global using System;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Collections.Generic;
global using System.Linq;
```

### Test Base Classes

```csharp
// tests/Shared/TestBase.cs
using Microsoft.EntityFrameworkCore;
using UserService.Infrastructure.Data;

namespace UserService.Tests.Shared
{
    public abstract class TestBase : IDisposable
    {
        protected readonly UserDbContext Context;
        
        protected TestBase()
        {
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
                
            Context = new UserDbContext(options);
        }
        
        protected async Task SeedDataAsync(params object[] entities)
        {
            Context.AddRange(entities);
            await Context.SaveChangesAsync();
        }
        
        public void Dispose()
        {
            Context.Dispose();
        }
    }
}
```

## 🎭 Mocking Strategy

### Repository Mocking

```csharp
public class MockRepositoryBuilder<T> where T : class
{
    private readonly Mock<T> _mock;
    
    public MockRepositoryBuilder()
    {
        _mock = new Mock<T>();
    }
    
    public MockRepositoryBuilder<T> Setup<TResult>(
        Expression<Func<T, Task<TResult>>> expression, 
        TResult returnValue)
    {
        _mock.Setup(expression).ReturnsAsync(returnValue);
        return this;
    }
    
    public MockRepositoryBuilder<T> SetupSequence<TResult>(
        Expression<Func<T, Task<TResult>>> expression, 
        params TResult[] returnValues)
    {
        var setup = _mock.SetupSequence(expression);
        foreach (var value in returnValues)
        {
            setup.ReturnsAsync(value);
        }
        return this;
    }
    
    public T Build() => _mock.Object;
    public Mock<T> Mock => _mock;
}

// Usage
var userRepository = new MockRepositoryBuilder<IUserRepository>()
    .Setup(x => x.ExistsAsync(It.IsAny<string>()), false)
    .Setup(x => x.CreateAsync(It.IsAny<User>()), user)
    .Build();
```

### HTTP Client Mocking

```csharp
// tests/ContentService.Tests/Shared/HttpClientMockBuilder.cs
using System.Net;
using System.Text;
using Moq;
using Moq.Protected;

namespace ContentService.Tests.Shared
{
    public class HttpClientMockBuilder
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        
        public HttpClientMockBuilder()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
        }
        
        public HttpClientMockBuilder SetupGet(string url, HttpStatusCode statusCode, string content)
        {
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.Method == HttpMethod.Get && 
                        req.RequestUri.ToString() == url),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content, Encoding.UTF8, "application/json")
                });
                
            return this;
        }
        
        public HttpClient Build()
        {
            return new HttpClient(_handlerMock.Object);
        }
    }
}
```

## 📊 Test Data Management

### Test Data Builders

```csharp
// tests/Shared/Builders/UserBuilder.cs
using UserService.Core.Entities;

namespace UserService.Tests.Shared.Builders
{
    public class UserBuilder
    {
        private User _user;
        
        public UserBuilder()
        {
            _user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "User",
                Email = $"test{Guid.NewGuid()}@example.com",
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            };
        }
        
        public UserBuilder WithId(Guid id)
        {
            _user.Id = id;
            return this;
        }
        
        public UserBuilder WithName(string firstName, string lastName)
        {
            _user.FirstName = firstName;
            _user.LastName = lastName;
            return this;
        }
        
        public UserBuilder WithEmail(string email)
        {
            _user.Email = email;
            return this;
        }
        
        public UserBuilder WithStatus(UserStatus status)
        {
            _user.Status = status;
            return this;
        }
        
        public UserBuilder CreatedAt(DateTime createdAt)
        {
            _user.CreatedAt = createdAt;
            return this;
        }
        
        public User Build() => _user;
        
        public static implicit operator User(UserBuilder builder) => builder.Build();
    }
}

// Usage
var user = new UserBuilder()
    .WithName("John", "Doe")
    .WithEmail("john.doe@example.com")
    .WithStatus(UserStatus.Active)
    .Build();
```

### Test Data Factory

```csharp
// tests/Shared/TestDataFactory.cs
namespace UserService.Tests.Shared
{
    public static class TestDataFactory
    {
        public static User CreateUser(
            string firstName = "Test",
            string lastName = "User",
            string email = null,
            UserStatus status = UserStatus.Active)
        {
            return new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email ?? $"test{Guid.NewGuid()}@example.com",
                Status = status,
                CreatedAt = DateTime.UtcNow
            };
        }
        
        public static List<User> CreateUsers(int count, UserStatus? status = null)
        {
            var users = new List<User>();
            for (int i = 0; i < count; i++)
            {
                users.Add(CreateUser(
                    firstName: $"User{i}",
                    lastName: "Test",
                    status: status ?? UserStatus.Active));
            }
            return users;
        }
        
        public static CreateUserCommand CreateUserCommand(
            string firstName = "Test",
            string lastName = "User",
            string email = null)
        {
            return new CreateUserCommand
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email ?? $"test{Guid.NewGuid()}@example.com"
            };
        }
    }
}
```

## 🚀 Performance Tests

### Load Testing with NBomber

```csharp
// tests/PerformanceTests/UserServiceLoadTests.cs
using NBomber.CSharp;
using NBomber.Http.CSharp;

namespace PerformanceTests
{
    public class UserServiceLoadTests
    {
        [Fact]
        public void CreateUser_LoadTest()
        {
            var scenario = Scenario.Create("create_user", async context =>
            {
                var request = new
                {
                    FirstName = "Load",
                    LastName = "Test",
                    Email = $"load{context.ScenarioInfo.CurrentInvocation}@example.com"
                };

                var response = await Http.CreateRequest("POST", "http://localhost:5001/api/users")
                    .WithJsonBody(request)
                    .WithHeader("Content-Type", "application/json")
                    .ExecuteAsync(context);

                return response;
            })
            .WithLoadSimulations(
                Simulation.InjectPerSec(rate: 10, during: TimeSpan.FromMinutes(2))
            );

            var stats = NBomberRunner
                .RegisterScenarios(scenario)
                .Run();

            // Assert performance requirements
            var createUserStats = stats.AllScenarioStats.First();
            Assert.True(createUserStats.Ok.Response.Mean < 500); // Average response time < 500ms
            Assert.True(createUserStats.Ok.Request.RPS > 8); // Min 8 requests per second
        }
    }
}
```

### Stress Testing

```csharp
public void CreateUser_StressTest()
{
    var scenario = Scenario.Create("stress_test", async context =>
    {
        // Same request logic
        var request = new { /* ... */ };
        var response = await Http.CreateRequest("POST", "http://localhost:5001/api/users")
            .WithJsonBody(request)
            .ExecuteAsync(context);
        return response;
    })
    .WithLoadSimulations(
        Simulation.RampingInject(
            rate: 50,
            interval: TimeSpan.FromSeconds(1),
            during: TimeSpan.FromMinutes(5))
    );

    var stats = NBomberRunner
        .RegisterScenarios(scenario)
        .Run();

    // Stress test assertions
    Assert.True(stats.AllScenarioStats.First().Fail.Request.Count == 0); // No failures
}
```

## 🔄 Test Automation

### GitHub Actions Workflow

```yaml
# .github/workflows/test.yml
name: Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_PASSWORD: postgres
          POSTGRES_DB: testdb
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432

    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore
      
    - name: Test
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
      
    - name: Upload coverage to Codecov
      uses: codecov/codecov-action@v3
      with:
        files: ./coverage.cobertura.xml
        
    - name: Integration Tests
      run: |
        dotnet run --project src/UserService/UserService.API &
        dotnet run --project src/ContentService/ContentService.API &
        sleep 30
        dotnet test tests/IntegrationTests --logger trx --results-directory TestResults/
        
    - name: API Tests
      run: |
        npm install -g newman
        newman run tests/postman/cms-microservices.json
```

### Test Coverage Configuration

```xml
<!-- Directory.Build.props -->
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>

  <ItemGroup Condition="'$(IsTestProject)' == 'true'">
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.1" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.4.5" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Moq" Version="4.20.69" />
    <PackageReference Include="coverlet.collector" Version="6.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
  </ItemGroup>
</Project>
```

### Coverage Report

```bash
# Generate coverage report
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Generate HTML report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

# View coverage
open coveragereport/index.html
```

---

Bu test kılavuzu, CMS mikroservis projesinin kapsamlı test stratejisini ve implementation'ını içermektedir. Test-driven development yaklaşımı ile quality assurance sağlanması hedeflenmektedir.
