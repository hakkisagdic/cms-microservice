using FluentAssertions;
using Moq;
using UserService.Core.DTOs;
using UserService.Core.Entities;
using UserService.Core.Features.Users.Queries;
using UserService.Core.Interfaces;
using Xunit;

namespace UserService.Tests.Unit;

public class GetAllUsersQueryHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly GetAllUsersQueryHandler _handler;

    public GetAllUsersQueryHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _handler = new GetAllUsersQueryHandler(_mockUserRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllUsers_WhenUsersExist()
    {

        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            }
        };

        var query = new GetAllUsersQuery();

        _mockUserRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
        result.Value!.First().FirstName.Should().Be("John");
        result.Value!.Last().FirstName.Should().Be("Jane");

        _mockUserRepository.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoUsersExist()
    {

        var users = new List<User>();
        var query = new GetAllUsersQuery();

        _mockUserRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().BeEmpty();

        _mockUserRepository.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnUsersWithDifferentStatuses_WhenMixedStatusUsersExist()
    {

        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Active",
                LastName = "User",
                Email = "active@example.com",
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Inactive",
                LastName = "User",
                Email = "inactive@example.com",
                Status = UserStatus.Inactive,
                CreatedAt = DateTime.UtcNow
            }
        };

        var query = new GetAllUsersQuery();

        _mockUserRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
        result.Value.Should().Contain(u => u.Status == UserStatus.Active);
        result.Value.Should().Contain(u => u.Status == UserStatus.Inactive);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRepositoryThrowsException()
    {

        var query = new GetAllUsersQuery();

        _mockUserRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        await FluentActions.Invoking(() => _handler.Handle(query, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Database connection failed");
    }

    [Fact]
    public async Task Handle_ShouldReturnUsersWithCompleteData_WhenUsersHaveAllFields()
    {

        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Complete",
                LastName = "User",
                Email = "complete@example.com",
                PhoneNumber = "1234567890",
                DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ProfileImageUrl = "profile.jpg",
                Bio = "Complete user bio",
                Department = "IT",
                Position = "Developer",
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var query = new GetAllUsersQuery();

        _mockUserRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(1);

        var user = result.Value!.First();
        user.PhoneNumber.Should().Be("1234567890");
        user.DateOfBirth.Should().NotBeNull();
        user.ProfileImageUrl.Should().Be("profile.jpg");
        user.Bio.Should().Be("Complete user bio");
        user.Department.Should().Be("IT");
        user.Position.Should().Be("Developer");
    }
}
