using FluentAssertions;
using Moq;
using UserService.Core.Entities;
using UserService.Core.Features.Users.Commands;
using UserService.Core.Interfaces;
using Xunit;

namespace UserService.Tests.Unit;

public class DeleteUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly DeleteUserCommandHandler _handler;

    public DeleteUserCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _handler = new DeleteUserCommandHandler(_mockUserRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteUser_WhenUserExists()
    {

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

        var command = new DeleteUserCommand(userId);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockUserRepository.Setup(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserDoesNotExist()
    {

        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found.");

        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenDeleteOperationFails()
    {

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

        var command = new DeleteUserCommand(userId);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockUserRepository.Setup(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Failed to delete user.");

        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRepositoryGetThrowsException()
    {

        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Database connection failed");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRepositoryDeleteThrowsException()
    {

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

        var command = new DeleteUserCommand(userId);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockUserRepository.Setup(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Delete operation failed"));

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Delete operation failed");
    }

    [Fact]
    public async Task Handle_ShouldDeleteInactiveUser_WhenUserIsInactive()
    {

        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@example.com",
            Status = UserStatus.Inactive,
            CreatedAt = DateTime.UtcNow
        };

        var command = new DeleteUserCommand(userId);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockUserRepository.Setup(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
