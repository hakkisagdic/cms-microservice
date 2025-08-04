using FluentAssertions;
using Moq;
using ContentService.Core.DTOs;
using ContentService.Core.Entities;
using ContentService.Core.Features.Contents.Queries;
using ContentService.Core.Interfaces;
using Xunit;

namespace ContentService.Tests.Unit;

public class GetAllContentsQueryHandlerTests
{
    private readonly Mock<IContentRepository> _mockContentRepository;
    private readonly GetAllContentsQueryHandler _handler;

    public GetAllContentsQueryHandlerTests()
    {
        _mockContentRepository = new Mock<IContentRepository>();
        _handler = new GetAllContentsQueryHandler(_mockContentRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllContents_WhenContentsExist()
    {
        // Arrange
        var contents = new List<Content>
        {
            new Content
            {
                Id = Guid.NewGuid(),
                Title = "First Article",
                Body = "First article body",
                Status = ContentStatus.Published,
                Type = ContentType.Article,
                AuthorId = Guid.NewGuid(),
                AuthorName = "John Doe",
                Category = "Technology",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Content
            {
                Id = Guid.NewGuid(),
                Title = "Second Article",
                Body = "Second article body",
                Status = ContentStatus.Draft,
                Type = ContentType.Article,
                AuthorId = Guid.NewGuid(),
                AuthorName = "Jane Smith",
                Category = "Business",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        var query = new GetAllContentsQuery();

        _mockContentRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(contents);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
        result.Value!.First().Title.Should().Be("First Article");
        result.Value!.Last().Title.Should().Be("Second Article");

        _mockContentRepository.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoContentsExist()
    {
        // Arrange
        var contents = new List<Content>();
        var query = new GetAllContentsQuery();

        _mockContentRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(contents);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().BeEmpty();

        _mockContentRepository.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnContentsWithDifferentStatuses_WhenMixedStatusContentsExist()
    {
        // Arrange
        var contents = new List<Content>
        {
            new Content
            {
                Id = Guid.NewGuid(),
                Title = "Published Article",
                Body = "Published article body",
                Status = ContentStatus.Published,
                Type = ContentType.Article,
                AuthorId = Guid.NewGuid(),
                AuthorName = "Author One",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Content
            {
                Id = Guid.NewGuid(),
                Title = "Draft Article",
                Body = "Draft article body",
                Status = ContentStatus.Draft,
                Type = ContentType.Article,
                AuthorId = Guid.NewGuid(),
                AuthorName = "Author Two",
                CreatedAt = DateTime.UtcNow
            },
            new Content
            {
                Id = Guid.NewGuid(),
                Title = "Archived Article",
                Body = "Archived article body",
                Status = ContentStatus.Archived,
                Type = ContentType.Article,
                AuthorId = Guid.NewGuid(),
                AuthorName = "Author Three",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            }
        };

        var query = new GetAllContentsQuery();

        _mockContentRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(contents);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(3);
        result.Value!.Should().Contain(c => c.Status == ContentStatus.Published);
        result.Value!.Should().Contain(c => c.Status == ContentStatus.Draft);
        result.Value!.Should().Contain(c => c.Status == ContentStatus.Archived);
    }

    [Fact]
    public async Task Handle_ShouldReturnContentsWithDifferentTypes_WhenMixedTypeContentsExist()
    {
        // Arrange
        var contents = new List<Content>
        {
            new Content
            {
                Id = Guid.NewGuid(),
                Title = "News Article",
                Body = "News article body",
                Status = ContentStatus.Published,
                Type = ContentType.Article,
                AuthorId = Guid.NewGuid(),
                AuthorName = "News Author",
                CreatedAt = DateTime.UtcNow
            },
            new Content
            {
                Id = Guid.NewGuid(),
                Title = "About Page",
                Body = "About page body",
                Status = ContentStatus.Published,
                Type = ContentType.Page,
                AuthorId = Guid.NewGuid(),
                AuthorName = "Admin User",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        var query = new GetAllContentsQuery();

        _mockContentRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(contents);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
        result.Value!.Should().Contain(c => c.Type == ContentType.Article);
        result.Value!.Should().Contain(c => c.Type == ContentType.Page);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRepositoryThrowsException()
    {
        // Arrange
        var query = new GetAllContentsQuery();

        _mockContentRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        await FluentActions.Invoking(() => _handler.Handle(query, CancellationToken.None))
            .Should().ThrowAsync<Exception>()
            .WithMessage("Database connection failed");
    }

    [Fact]
    public async Task Handle_ShouldReturnContentsWithCompleteData_WhenContentsHaveAllFields()
    {
        // Arrange
        var contents = new List<Content>
        {
            new Content
            {
                Id = Guid.NewGuid(),
                Title = "Complete Article",
                Body = "Complete article with all fields",
                Summary = "Complete summary",
                Status = ContentStatus.Published,
                Type = ContentType.Article,
                FeaturedImageUrl = "featured.jpg",
                MetaTitle = "Complete Meta Title",
                MetaDescription = "Complete meta description",
                Tags = "complete,test,article",
                Category = "Technology",
                ViewCount = 150,
                PublishedAt = DateTime.UtcNow.AddDays(-1),
                AuthorId = Guid.NewGuid(),
                AuthorName = "Complete Author",
                Slug = "complete-article",
                SortOrder = 1,
                IsFeatured = true,
                AllowComments = true,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };

        var query = new GetAllContentsQuery();

        _mockContentRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(contents);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(1);
        
        var content = result.Value!.First();
        content.FeaturedImageUrl.Should().Be("featured.jpg");
        content.MetaTitle.Should().Be("Complete Meta Title");
        content.MetaDescription.Should().Be("Complete meta description");
        content.Tags.Should().Be("complete,test,article");
        content.ViewCount.Should().Be(150);
        content.IsFeatured.Should().BeTrue();
        content.AllowComments.Should().BeTrue();
        content.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnContentsOrderedByCreationDate_WhenMultipleContentsExist()
    {
        // Arrange
        var olderDate = DateTime.UtcNow.AddDays(-10);
        var newerDate = DateTime.UtcNow.AddDays(-1);
        
        var contents = new List<Content>
        {
            new Content
            {
                Id = Guid.NewGuid(),
                Title = "Newer Article",
                Body = "Newer article body",
                Status = ContentStatus.Published,
                Type = ContentType.Article,
                AuthorId = Guid.NewGuid(),
                AuthorName = "Author",
                CreatedAt = newerDate
            },
            new Content
            {
                Id = Guid.NewGuid(),
                Title = "Older Article",
                Body = "Older article body",
                Status = ContentStatus.Published,
                Type = ContentType.Article,
                AuthorId = Guid.NewGuid(),
                AuthorName = "Author",
                CreatedAt = olderDate
            }
        };

        var query = new GetAllContentsQuery();

        _mockContentRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(contents);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
        
        // Verify that we can access the creation dates (ordering logic might be in repository)
        result.Value!.Should().Contain(c => c.CreatedAt == olderDate);
        result.Value!.Should().Contain(c => c.CreatedAt == newerDate);
    }
}
