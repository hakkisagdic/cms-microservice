using FluentAssertions;
using Moq;
using UserService.Core.DTOs;
using UserService.Core.Entities;
using UserService.Core.Features.Users.Queries;
using UserService.Core.Interfaces;
using Xunit;

namespace UserService.Tests.Unit;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _handler = new GetUserByIdQueryHandler(_mockUserRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUser_WhenUserExists()
    {

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var query = new GetUserByIdQuery(userId);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(userId);
        result.Value.FirstName.Should().Be(user.FirstName);
        result.Value.LastName.Should().Be(user.LastName);
        result.Value.Email.Should().Be(user.Email);

        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserDoesNotExist()
    {

        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found.");

        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnUserWithAllFields_WhenUserHasCompleteData()
    {

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "1234567890",
            DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ProfileImageUrl = "profile.jpg",
            Bio = "Software Developer",
            Department = "IT",
            Position = "Senior Developer",
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var query = new GetUserByIdQuery(userId);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(userId);
        result.Value.FirstName.Should().Be(user.FirstName);
        result.Value.LastName.Should().Be(user.LastName);
        result.Value.Email.Should().Be(user.Email);
        result.Value.PhoneNumber.Should().Be(user.PhoneNumber);
        result.Value.DateOfBirth.Should().Be(user.DateOfBirth);
        result.Value.ProfileImageUrl.Should().Be(user.ProfileImageUrl);
        result.Value.Bio.Should().Be(user.Bio);
        result.Value.Department.Should().Be(user.Department);
        result.Value.Position.Should().Be(user.Position);
        result.Value.Status.Should().Be(user.Status);
        result.Value.CreatedAt.Should().Be(user.CreatedAt);
        result.Value.UpdatedAt.Should().Be(user.UpdatedAt);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRepositoryThrowsException()
    {

        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        await FluentActions.Invoking(() => _handler.Handle(query, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Database connection failed");
    }

    [Fact]
    public async Task Handle_ShouldReturnInactiveUser_WhenUserIsInactive()
    {

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@example.com",
            Status = UserStatus.Inactive,
            CreatedAt = DateTime.UtcNow
        };

        var query = new GetUserByIdQuery(userId);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be(UserStatus.Inactive);
    }
}
