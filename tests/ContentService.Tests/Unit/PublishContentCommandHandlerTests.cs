using FluentAssertions;
using Moq;
using ContentService.Core.Features.Contents.Commands;
using ContentService.Core.Interfaces;
using ContentService.Core.Entities;
using ContentService.Core.DTOs;
using Xunit;

namespace ContentService.Tests.Unit;

public class PublishContentCommandHandlerTests
{
    private readonly Mock<IContentRepository> _mockContentRepository;
    private readonly PublishContentCommandHandler _handler;

    public PublishContentCommandHandlerTests()
    {
        _mockContentRepository = new Mock<IContentRepository>();
        _handler = new PublishContentCommandHandler(_mockContentRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldPublishContent_WhenContentExists()
    {

        var contentId = Guid.NewGuid();
        var draftContent = new Content
        {
            Id = contentId,
            Title = "Draft Content",
            Body = "Draft Body",
            Status = ContentStatus.Draft,
            Type = ContentType.Article,
            AuthorId = Guid.NewGuid(),
            AuthorName = "Author",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            PublishedAt = null
        };

        var publishData = new PublishContentDto(DateTime.UtcNow.AddHours(1));
        var command = new PublishContentCommand(contentId, publishData);

        _mockContentRepository.Setup(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(draftContent);

        _mockContentRepository.Setup(x => x.UpdateAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Content content, CancellationToken ct) => content);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be(ContentStatus.Published);
        result.Value.PublishedAt.Should().NotBeNull();
        result.Value.PublishedAt.Should().BeCloseTo(publishData.PublishAt!.Value, TimeSpan.FromSeconds(1));

        _mockContentRepository.Verify(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()), Times.Once);
        _mockContentRepository.Verify(x => x.UpdateAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPublishContentNow_WhenNoPublishDateProvided()
    {

        var contentId = Guid.NewGuid();
        var draftContent = new Content
        {
            Id = contentId,
            Title = "Draft Content",
            Body = "Draft Body",
            Status = ContentStatus.Draft,
            Type = ContentType.Article,
            AuthorId = Guid.NewGuid(),
            AuthorName = "Author",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            PublishedAt = null
        };

        var publishData = new PublishContentDto(null);
        var command = new PublishContentCommand(contentId, publishData);

        _mockContentRepository.Setup(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(draftContent);

        _mockContentRepository.Setup(x => x.UpdateAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Content content, CancellationToken ct) => content);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be(ContentStatus.Published);
        result.Value.PublishedAt.Should().NotBeNull();
        result.Value.PublishedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        _mockContentRepository.Verify(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()), Times.Once);
        _mockContentRepository.Verify(x => x.UpdateAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenContentDoesNotExist()
    {

        var contentId = Guid.NewGuid();
        var publishData = new PublishContentDto(DateTime.UtcNow);
        var command = new PublishContentCommand(contentId, publishData);

        _mockContentRepository.Setup(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Content?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Content not found.");

        _mockContentRepository.Verify(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()), Times.Once);
        _mockContentRepository.Verify(x => x.UpdateAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRepublishContent_WhenContentIsAlreadyPublished()
    {

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
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            PublishedAt = DateTime.UtcNow.AddDays(-1)
        };

        var newPublishDate = DateTime.UtcNow.AddHours(2);
        var publishData = new PublishContentDto(newPublishDate);
        var command = new PublishContentCommand(contentId, publishData);

        _mockContentRepository.Setup(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publishedContent);

        _mockContentRepository.Setup(x => x.UpdateAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Content content, CancellationToken ct) => content);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be(ContentStatus.Published);
        result.Value.PublishedAt.Should().NotBeNull();
        result.Value.PublishedAt.Should().BeCloseTo(newPublishDate, TimeSpan.FromSeconds(1));

        _mockContentRepository.Verify(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()), Times.Once);
        _mockContentRepository.Verify(x => x.UpdateAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRepositoryFails()
    {

        var contentId = Guid.NewGuid();
        var publishData = new PublishContentDto(DateTime.UtcNow);
        var command = new PublishContentCommand(contentId, publishData);

        _mockContentRepository.Setup(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        await Assert.ThrowsAsync<Exception>(async () =>
            await _handler.Handle(command, CancellationToken.None));

        _mockContentRepository.Verify(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetUpdatedAt_WhenContentIsPublished()
    {

        var contentId = Guid.NewGuid();
        var draftContent = new Content
        {
            Id = contentId,
            Title = "Draft Content",
            Body = "Draft Body",
            Status = ContentStatus.Draft,
            Type = ContentType.Article,
            AuthorId = Guid.NewGuid(),
            AuthorName = "Author",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = null,
            PublishedAt = null
        };

        var publishData = new PublishContentDto(DateTime.UtcNow);
        var command = new PublishContentCommand(contentId, publishData);

        _mockContentRepository.Setup(x => x.GetByIdAsync(contentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(draftContent);

        _mockContentRepository.Setup(x => x.UpdateAsync(It.IsAny<Content>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Content content, CancellationToken ct) => content);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.UpdatedAt.Should().NotBeNull();
        result.Value.UpdatedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        _mockContentRepository.Verify(x => x.UpdateAsync(It.Is<Content>(c =>
            c.UpdatedAt != null &&
            c.Status == ContentStatus.Published &&
            c.PublishedAt != null), It.IsAny<CancellationToken>()), Times.Once);
    }
}
