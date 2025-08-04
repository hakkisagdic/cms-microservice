using FluentAssertions;
using Moq;
using ContentService.Core.DTOs;
using ContentService.Core.Entities;
using ContentService.Core.Features.Contents.Queries;
using ContentService.Core.Interfaces;
using Xunit;

namespace ContentService.Tests.Unit;

public class GetContentByIdQueryHandlerTests
{
    private readonly Mock<IContentRepository> _mockContentRepository;
    private readonly GetContentByIdQueryHandler _handler;

    public GetContentByIdQueryHandlerTests()
    {
        _mockContentRepository = new Mock<IContentRepository>();
        _handler = new GetContentByIdQueryHandler(_mockContentRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnContent_WhenContentExists()
    {
        // Arrange
        var contentId = Guid.NewGuid();
        var content = new Content
        {
            Id = contentId,
            Title = "Test Article",
            Body = "This is a test article body",
            Summary = "Test summary",
            Status = ContentStatus.Published,
            Type = ContentType.Article,
            AuthorId = Guid.NewGuid(),
            AuthorName = "John Doe",
            Category = "Technology",
            Slug = "test-article",
            ViewCount = 10,
            CreatedAt = DateTime.UtcNow
        };

        var query = new GetContentByIdQuery(contentId);

        _mockContentRepository.Setup(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(contentId);
        result.Value.Title.Should().Be(content.Title);
        result.Value.Body.Should().Be(content.Body);
        result.Value.Summary.Should().Be(content.Summary);
        result.Value.Status.Should().Be(content.Status);
        result.Value.AuthorName.Should().Be(content.AuthorName);
        result.Value.Category.Should().Be(content.Category);
        result.Value.ViewCount.Should().Be(content.ViewCount);

        _mockContentRepository.Verify(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenContentDoesNotExist()
    {
        // Arrange
        var contentId = Guid.NewGuid();
        var query = new GetContentByIdQuery(contentId);

        _mockContentRepository.Setup(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Content?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Content not found.");

        _mockContentRepository.Verify(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnContentWithAllFields_WhenContentHasCompleteData()
    {
        // Arrange
        var contentId = Guid.NewGuid();
        var publishedAt = DateTime.UtcNow.AddDays(-1);
        var content = new Content
        {
            Id = contentId,
            Title = "Complete Article",
            Body = "This is a complete article with all fields",
            Summary = "Complete summary",
            Status = ContentStatus.Published,
            Type = ContentType.Article,
            FeaturedImageUrl = "featured-image.jpg",
            MetaTitle = "Complete Article Meta",
            MetaDescription = "Complete article meta description",
            Tags = "tech,programming,testing",
            Category = "Technology",
            ViewCount = 100,
            PublishedAt = publishedAt,
            AuthorId = Guid.NewGuid(),
            AuthorName = "Jane Smith",
            Slug = "complete-article",
            SortOrder = 1,
            IsFeatured = true,
            AllowComments = true,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };

        var query = new GetContentByIdQuery(contentId);

        _mockContentRepository.Setup(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(contentId);
        result.Value.Title.Should().Be(content.Title);
        result.Value.Body.Should().Be(content.Body);
        result.Value.Summary.Should().Be(content.Summary);
        result.Value.Status.Should().Be(content.Status);
        result.Value.Type.Should().Be(content.Type);
        result.Value.FeaturedImageUrl.Should().Be(content.FeaturedImageUrl);
        result.Value.MetaTitle.Should().Be(content.MetaTitle);
        result.Value.MetaDescription.Should().Be(content.MetaDescription);
        result.Value.Tags.Should().Be(content.Tags);
        result.Value.Category.Should().Be(content.Category);
        result.Value.ViewCount.Should().Be(content.ViewCount);
        result.Value.PublishedAt.Should().Be(content.PublishedAt);
        result.Value.AuthorId.Should().Be(content.AuthorId);
        result.Value.AuthorName.Should().Be(content.AuthorName);
        result.Value.Slug.Should().Be(content.Slug);
        result.Value.SortOrder.Should().Be(content.SortOrder);
        result.Value.IsFeatured.Should().Be(content.IsFeatured);
        result.Value.AllowComments.Should().Be(content.AllowComments);
        result.Value.CreatedAt.Should().Be(content.CreatedAt);
        result.Value.UpdatedAt.Should().Be(content.UpdatedAt);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRepositoryThrowsException()
    {
        // Arrange
        var contentId = Guid.NewGuid();
        var query = new GetContentByIdQuery(contentId);

        _mockContentRepository.Setup(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        await FluentActions.Invoking(() => _handler.Handle(query, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Database connection failed");
    }

    [Fact]
    public async Task Handle_ShouldReturnDraftContent_WhenContentIsDraft()
    {
        // Arrange
        var contentId = Guid.NewGuid();
        var content = new Content
        {
            Id = contentId,
            Title = "Draft Article",
            Body = "This is a draft article",
            Status = ContentStatus.Draft,
            Type = ContentType.Article,
            AuthorId = Guid.NewGuid(),
            AuthorName = "Author Name",
            CreatedAt = DateTime.UtcNow
        };

        var query = new GetContentByIdQuery(contentId);

        _mockContentRepository.Setup(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be(ContentStatus.Draft);
        result.Value.PublishedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnPageContent_WhenContentIsPage()
    {
        // Arrange
        var contentId = Guid.NewGuid();
        var content = new Content
        {
            Id = contentId,
            Title = "About Page",
            Body = "This is the about page content",
            Status = ContentStatus.Published,
            Type = ContentType.Page,
            AuthorId = Guid.NewGuid(),
            AuthorName = "Admin User",
            Slug = "about",
            CreatedAt = DateTime.UtcNow
        };

        var query = new GetContentByIdQuery(contentId);

        _mockContentRepository.Setup(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Type.Should().Be(ContentType.Page);
        result.Value.Title.Should().Be("About Page");
    }
}
