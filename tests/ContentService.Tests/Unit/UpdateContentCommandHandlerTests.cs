using FluentAssertions;
using Moq;
using ContentService.Core.Features.Contents.Commands;
using ContentService.Core.Interfaces;
using ContentService.Core.Entities;
using ContentService.Core.DTOs;
using Xunit;

namespace ContentService.Tests.Unit;

public class UpdateContentCommandHandlerTests
{
    private readonly Mock<IContentRepository> _mockContentRepository;
    private readonly UpdateContentCommandHandler _handler;

    public UpdateContentCommandHandlerTests()
    {
        _mockContentRepository = new Mock<IContentRepository>();
        _handler = new UpdateContentCommandHandler(_mockContentRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateContent_WhenContentExists()
    {
        // Arrange
        var contentId = Guid.NewGuid();
        var existingContent = new Content
        {
            Id = contentId,
            Title = "Old Title",
            Body = "Old Body",
            Summary = "Old Summary",
            Status = ContentStatus.Draft,
            Type = ContentType.Article,
            AuthorId = Guid.NewGuid(),
            AuthorName = "Author",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ViewCount = 5
        };

        var updateDto = new UpdateContentDto(
            "Updated Title",
            "Updated Body",
            "Updated Summary",
            ContentStatus.Published,
            ContentType.BlogPost,
            "updated-image.jpg",
            "Updated Meta Title",
            "Updated Meta Description",
            "tag1,tag2,tag3",
            "Updated Category",
            "updated-slug",
            2,
            true,
            false
        );

        var command = new UpdateContentCommand(contentId, updateDto);

        _mockContentRepository.Setup(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingContent);

        _mockContentRepository.Setup(x => x.UpdateAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Content content, CancellationToken ct) => content);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be("Updated Title");
        result.Value.Body.Should().Be("Updated Body");
        result.Value.Summary.Should().Be("Updated Summary");
        result.Value.Status.Should().Be(ContentStatus.Published);
        result.Value.Type.Should().Be(ContentType.BlogPost);

        _mockContentRepository.Verify(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()), Times.Once);
        _mockContentRepository.Verify(x => x.UpdateAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenContentDoesNotExist()
    {
        // Arrange
        var contentId = Guid.NewGuid();
        var updateDto = new UpdateContentDto(
            "Updated Title",
            "Updated Body",
            null,
            ContentStatus.Published,
            ContentType.Article,
            null,
            null,
            null,
            null,
            null,
            null,
            0,
            false,
            true
        );

        var command = new UpdateContentCommand(contentId, updateDto);

        _mockContentRepository.Setup(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Content?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Content not found.");

        _mockContentRepository.Verify(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()), Times.Once);
        _mockContentRepository.Verify(x => x.UpdateAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRepositoryFails()
    {
        // Arrange
        var contentId = Guid.NewGuid();
        var updateDto = new UpdateContentDto(
            "Updated Title",
            "Updated Body",
            null,
            ContentStatus.Published,
            ContentType.Article,
            null,
            null,
            null,
            null,
            null,
            null,
            0,
            false,
            true
        );

        var command = new UpdateContentCommand(contentId, updateDto);

        _mockContentRepository.Setup(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () =>
            await _handler.Handle(command, CancellationToken.None));

        _mockContentRepository.Verify(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
