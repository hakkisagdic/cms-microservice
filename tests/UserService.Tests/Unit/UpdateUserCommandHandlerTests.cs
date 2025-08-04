using FluentAssertions;
using Moq;
using UserService.Core.DTOs;
using UserService.Core.Entities;
using UserService.Core.Features.Users.Commands;
using UserService.Core.Interfaces;
using Xunit;

namespace UserService.Tests.Unit;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _handler = new UpdateUserCommandHandler(_mockUserRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateUser_WhenUserExistsAndValidRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var updateUserDto = new UpdateUserDto(
            "Johnny",
            "Updated",
            existingUser.Email,
            "1234567890",
            new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UserStatus.Active,
            "new-profile.jpg",
            "Updated bio",
            "Engineering",
            "Senior Developer"
        );

        var command = new UpdateUserCommand(userId, updateUserDto);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var updatedUser = new User
        {
            Id = userId,
            FirstName = updateUserDto.FirstName,
            LastName = updateUserDto.LastName,
            Email = existingUser.Email,
            PhoneNumber = updateUserDto.PhoneNumber,
            DateOfBirth = updateUserDto.DateOfBirth,
            ProfileImageUrl = updateUserDto.ProfileImageUrl,
            Bio = updateUserDto.Bio,
            Department = updateUserDto.Department,
            Position = updateUserDto.Position,
            Status = existingUser.Status,
            CreatedAt = existingUser.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FirstName.Should().Be(updateUserDto.FirstName);
        result.Value.LastName.Should().Be(updateUserDto.LastName);
        result.Value.PhoneNumber.Should().Be(updateUserDto.PhoneNumber);
        result.Value.Bio.Should().Be(updateUserDto.Bio);
        result.Value.Department.Should().Be(updateUserDto.Department);
        result.Value.Position.Should().Be(updateUserDto.Position);

        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateUserDto = new UpdateUserDto(
            "Johnny",
            "Updated",
            "john.doe@example.com",
            null,
            null,
            UserStatus.Active,
            null,
            null,
            null,
            null
        );

        var command = new UpdateUserCommand(userId, updateUserDto);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found.");

        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUpdateOnlyProvidedFields_WhenPartialUpdate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "0987654321",
            Bio = "Original bio",
            Department = "IT",
            Position = "Developer",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var updateUserDto = new UpdateUserDto(
            "Johnny", // Only update FirstName
            existingUser.LastName,     // Keep existing LastName
            existingUser.Email,
            existingUser.PhoneNumber,     // Keep existing PhoneNumber
            existingUser.DateOfBirth,     // Keep existing DateOfBirth
            existingUser.Status,
            existingUser.ProfileImageUrl,     // Keep existing ProfileImageUrl
            "Updated bio", // Update Bio
            existingUser.Department,     // Keep existing Department
            existingUser.Position      // Keep existing Position
        );

        var command = new UpdateUserCommand(userId, updateUserDto);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken _) => user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FirstName.Should().Be("Johnny");
        result.Value.LastName.Should().Be("Doe"); // Should remain unchanged
        result.Value.PhoneNumber.Should().Be("0987654321"); // Should remain unchanged
        result.Value.Bio.Should().Be("Updated bio");
        result.Value.Department.Should().Be("IT"); // Should remain unchanged
        result.Value.Position.Should().Be("Developer"); // Should remain unchanged
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRepositoryGetThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateUserDto = new UpdateUserDto("John", "Doe", "john@example.com", null, null, UserStatus.Active, null, null, null, null);
        var command = new UpdateUserCommand(userId, updateUserDto);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Database connection failed");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRepositoryUpdateThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var updateUserDto = new UpdateUserDto("Johnny", "Updated", "johnny@example.com", null, null, UserStatus.Active, null, null, null, null);
        var command = new UpdateUserCommand(userId, updateUserDto);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Update failed"));

        // Act & Assert
        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Update failed");
    }
}
