using FluentAssertions;
using Moq;
using UserService.Core.DTOs;
using UserService.Core.Entities;
using UserService.Core.Features.Users.Commands;
using UserService.Core.Interfaces;
using Xunit;

namespace UserService.Tests.Unit;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _handler = new CreateUserCommandHandler(_mockUserRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateUser_WhenValidRequest()
    {
        // Arrange
        var createUserDto = new CreateUserDto(
            "John",
            "Doe",
            "john.doe@example.com",
            "1234567890",
            new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            "profile.jpg",
            "Bio",
            "IT",
            "Developer"
        );

        var command = new CreateUserCommand(createUserDto);

        _mockUserRepository.Setup(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var expectedUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            Email = createUserDto.Email,
            PhoneNumber = createUserDto.PhoneNumber,
            DateOfBirth = createUserDto.DateOfBirth,
            ProfileImageUrl = createUserDto.ProfileImageUrl,
            Bio = createUserDto.Bio,
            Department = createUserDto.Department,
            Position = createUserDto.Position,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FirstName.Should().Be(createUserDto.FirstName);
        result.Value.LastName.Should().Be(createUserDto.LastName);
        result.Value.Email.Should().Be(createUserDto.Email);
        result.Value.Status.Should().Be(UserStatus.Active);

        _mockUserRepository.Verify(x => x.EmailExistsAsync(createUserDto.Email, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenEmailAlreadyExists()
    {
        // Arrange
        var createUserDto = new CreateUserDto(
            "John",
            "Doe",
            "john.doe@example.com",
            null,
            null,
            null,
            null,
            null,
            null
        );

        var command = new CreateUserCommand(createUserDto);

        _mockUserRepository.Setup(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("A user with this email already exists.");

        _mockUserRepository.Verify(x => x.EmailExistsAsync(createUserDto.Email, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateUserWithMinimalData_WhenOptionalFieldsAreNull()
    {
        // Arrange
        var createUserDto = new CreateUserDto(
            "Jane",
            "Smith",
            "jane.smith@example.com",
            null,
            null,
            null,
            null,
            null,
            null
        );

        var command = new CreateUserCommand(createUserDto);

        _mockUserRepository.Setup(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var expectedUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            Email = createUserDto.Email,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FirstName.Should().Be(createUserDto.FirstName);
        result.Value.LastName.Should().Be(createUserDto.LastName);
        result.Value.Email.Should().Be(createUserDto.Email);
        result.Value.PhoneNumber.Should().BeNull();
        result.Value.DateOfBirth.Should().BeNull();
        result.Value.ProfileImageUrl.Should().BeNull();
        result.Value.Bio.Should().BeNull();
        result.Value.Department.Should().BeNull();
        result.Value.Position.Should().BeNull();
        result.Value.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenRepositoryThrowsException()
    {
        // Arrange
        var createUserDto = new CreateUserDto(
            "John",
            "Doe",
            "john.doe@example.com",
            null,
            null,
            null,
            null,
            null,
            null
        );

        var command = new CreateUserCommand(createUserDto);

        _mockUserRepository.Setup(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Database connection failed");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenEmailCheckThrowsException()
    {
        // Arrange
        var createUserDto = new CreateUserDto(
            "John",
            "Doe",
            "john.doe@example.com",
            null,
            null,
            null,
            null,
            null,
            null
        );

        var command = new CreateUserCommand(createUserDto);

        _mockUserRepository.Setup(x => x.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Email validation failed"));

        // Act & Assert
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Email validation failed");
    }
}
