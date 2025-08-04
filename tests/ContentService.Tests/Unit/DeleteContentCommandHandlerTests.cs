using FluentAssertions;
using Moq;
using ContentService.Core.Features.Contents.Commands;
using ContentService.Core.Interfaces;
using ContentService.Core.Entities;
using Xunit;

namespace ContentService.Tests.Unit;

public class DeleteContentCommandHandlerTests
{
    private readonly Mock<IContentRepository> _mockContentRepository;
    private readonly DeleteContentCommandHandler _handler;

    public DeleteContentCommandHandlerTests()
    {
        _mockContentRepository = new Mock<IContentRepository>();
        _handler = new DeleteContentCommandHandler(_mockContentRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteContent_WhenContentExists()
    {
        // Arrange
        var contentId = Guid.NewGuid();
        var existingContent = new Content
        {
            Id = contentId,
            Title = "Test Content",
            Body = "Test Body",
            Status = ContentStatus.Draft,
            Type = ContentType.Article,
            AuthorId = Guid.NewGuid(),
            AuthorName = "Author",
            CreatedAt = DateTime.UtcNow
        };

        var command = new DeleteContentCommand(contentId);

        _mockContentRepository.Setup(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingContent);

        _mockContentRepository.Setup(x => x.DeleteAsync(contentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockContentRepository.Verify(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()), Times.Once);
        _mockContentRepository.Verify(x => x.DeleteAsync(contentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenContentDoesNotExist()
    {
        // Arrange
        var contentId = Guid.NewGuid();
        var command = new DeleteContentCommand(contentId);

        _mockContentRepository.Setup(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Content?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Content not found.");

        _mockContentRepository.Verify(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()), Times.Once);
        _mockContentRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRepositoryFails()
    {
        // Arrange
        var contentId = Guid.NewGuid();
        var command = new DeleteContentCommand(contentId);

        _mockContentRepository.Setup(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () =>
            await _handler.Handle(command, CancellationToken.None));

        _mockContentRepository.Verify(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldDeletePublishedContent_WhenContentIsPublished()
    {
        // Arrange
        var contentId = Guid.NewGuid();
        var publishedContent = new Content
        {
            Id = contentId,
            Title = "Published Content",
            Body = "Published Body",
            Status = ContentStatus.Published,
            Type = ContentType.Article,
            AuthorId = Guid.NewGuid(),
            AuthorName = "Author",
            CreatedAt = DateTime.UtcNow,
            PublishedAt = DateTime.UtcNow
        };

        var command = new DeleteContentCommand(contentId);

        _mockContentRepository.Setup(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publishedContent);

        _mockContentRepository.Setup(x => x.DeleteAsync(contentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockContentRepository.Verify(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()), Times.Once);
        _mockContentRepository.Verify(x => x.DeleteAsync(contentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldDeleteArchivedContent_WhenContentIsArchived()
    {
        // Arrange
        var contentId = Guid.NewGuid();
        var archivedContent = new Content
        {
            Id = contentId,
            Title = "Archived Content",
            Body = "Archived Body",
            Status = ContentStatus.Archived,
            Type = ContentType.Article,
            AuthorId = Guid.NewGuid(),
            AuthorName = "Author",
            CreatedAt = DateTime.UtcNow
        };

        var command = new DeleteContentCommand(contentId);

        _mockContentRepository.Setup(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(archivedContent);

        _mockContentRepository.Setup(x => x.DeleteAsync(contentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockContentRepository.Verify(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()), Times.Once);
        _mockContentRepository.Verify(x => x.DeleteAsync(contentId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
