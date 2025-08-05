using FluentAssertions;
using Moq;
using ContentService.Core.DTOs;
using ContentService.Core.Entities;
using ContentService.Core.Features.Contents.Commands;
using ContentService.Core.Interfaces;
using UserService.Core.DTOs;
using UserService.Core.Entities;
using Xunit;

namespace ContentService.Tests.Unit;

public class CreateContentCommandHandlerTests
{
    private readonly Mock<IContentRepository> _mockContentRepository;
    private readonly Mock<IUserServiceClient> _mockUserService;
    private readonly CreateContentCommandHandler _handler;

    public CreateContentCommandHandlerTests()
    {
        _mockContentRepository = new Mock<IContentRepository>();
        _mockUserService = new Mock<IUserServiceClient>();
        _handler = new CreateContentCommandHandler(_mockContentRepository.Object, _mockUserService.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateContent_WhenValidRequestAndAuthorExists()
    {

        var authorId = Guid.NewGuid();
        var createContentDto = new CreateContentDto(
            "Test Article",
            "This is a test article body",
            "Test summary",
            ContentType.Article,
            null,
            null,
            null,
            null,
            "Technology",
            authorId,
            null,
            0,
            false,
            true
        );

        var command = new CreateContentCommand(createContentDto);

        var authorDto = new ContentService.Core.Interfaces.UserDto(
            authorId,
            "John",
            "Doe",
            "john.doe@example.com"
        );

        _mockUserService.Setup(x => x.UserExistsAsync(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockUserService.Setup(x => x.GetUserByIdAsync(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authorDto);

        _mockContentRepository.Setup(x => x.SlugExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var expectedContent = new Content
        {
            Id = Guid.NewGuid(),
            Title = createContentDto.Title,
            Body = createContentDto.Body,
            Summary = createContentDto.Summary,
            AuthorId = createContentDto.AuthorId,
            AuthorName = $"{authorDto.FirstName} {authorDto.LastName}",
            Category = createContentDto.Category,
            Status = ContentStatus.Draft,
            Type = ContentType.Article,
            Slug = "test-article",
            CreatedAt = DateTime.UtcNow
        };

        _mockContentRepository.Setup(x => x.AddAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedContent);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be(createContentDto.Title);
        result.Value.Body.Should().Be(createContentDto.Body);
        result.Value.Summary.Should().Be(createContentDto.Summary);
        result.Value.AuthorId.Should().Be(createContentDto.AuthorId);
        result.Value.AuthorName.Should().Be("John Doe");
        result.Value.Category.Should().Be(createContentDto.Category);
        result.Value.Status.Should().Be(ContentStatus.Draft);

        _mockUserService.Verify(x => x.UserExistsAsync(authorId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserService.Verify(x => x.GetUserByIdAsync(authorId, It.IsAny<CancellationToken>()), Times.Once);
        _mockContentRepository.Verify(x => x.AddAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenAuthorDoesNotExist()
    {

        var authorId = Guid.NewGuid();
        var createContentDto = new CreateContentDto(
            "Test Article",
            "This is a test article body",
            "Test summary",
            ContentType.Article,
            null,
            null,
            null,
            null,
            "Technology",
            authorId,
            null,
            0,
            false,
            true
        );

        var command = new CreateContentCommand(createContentDto);

        _mockUserService.Setup(x => x.UserExistsAsync(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Author not found.");

        _mockUserService.Verify(x => x.UserExistsAsync(authorId, It.IsAny<CancellationToken>()), Times.Once);
        _mockUserService.Verify(x => x.GetUserByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockContentRepository.Verify(x => x.AddAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
