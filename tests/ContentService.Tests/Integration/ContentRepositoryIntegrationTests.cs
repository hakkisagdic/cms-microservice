using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ContentService.Core.Entities;
using ContentService.Infrastructure.Data;
using ContentService.Infrastructure.Repositories;
using Xunit;

namespace ContentService.Tests.Integration;

public class ContentRepositoryIntegrationTests : IDisposable
{
    private readonly ContentDbContext _context;
    private readonly ContentRepository _repository;

    public ContentRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
        _repository = new ContentRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddContent_WhenValidContent()
    {

        var content = new Content
        {
            Title = "Test Content",
            Body = "Test Body",
            Summary = "Test Summary",
            Type = ContentType.Article,
            Status = ContentStatus.Draft,
            AuthorId = Guid.NewGuid(),
            AuthorName = "Test Author",
            Slug = "test-content",
            Category = "Technology",
            CreatedAt = DateTime.UtcNow
        };

        var result = await _repository.AddAsync(content, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Title.Should().Be("Test Content");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        var savedContent = await _context.Contents.FindAsync(result.Id);
        savedContent.Should().NotBeNull();
        savedContent!.Title.Should().Be("Test Content");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnContent_WhenContentExists()
    {

        var content = new Content
        {
            Title = "Test Content",
            Body = "Test Body",
            Summary = "Test Summary",
            Type = ContentType.Article,
            Status = ContentStatus.Draft,
            AuthorId = Guid.NewGuid(),
            AuthorName = "Test Author",
            Slug = "test-content",
            Category = "Technology",
            CreatedAt = DateTime.UtcNow
        };

        _context.Contents.Add(content);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(content.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(content.Id);
        result.Title.Should().Be("Test Content");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenContentDoesNotExist()
    {

        var nonExistentId = Guid.NewGuid();

        var result = await _repository.GetByIdAsync(nonExistentId, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllContents()
    {

        var contents = new List<Content>
        {
            new Content
            {
                Title = "Content 1",
                Body = "Body 1",
                Type = ContentType.Article,
                Status = ContentStatus.Published,
                AuthorId = Guid.NewGuid(),
                AuthorName = "Author 1",
                Slug = "content-1",
                CreatedAt = DateTime.UtcNow
            },
            new Content
            {
                Title = "Content 2",
                Body = "Body 2",
                Type = ContentType.Page,
                Status = ContentStatus.Draft,
                AuthorId = Guid.NewGuid(),
                AuthorName = "Author 2",
                Slug = "content-2",
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Contents.AddRange(contents);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync(CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Select(c => c.Title).Should().Contain(new[] { "Content 1", "Content 2" });
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateContent_WhenContentExists()
    {

        var content = new Content
        {
            Title = "Original Title",
            Body = "Original Body",
            Type = ContentType.Article,
            Status = ContentStatus.Draft,
            AuthorId = Guid.NewGuid(),
            AuthorName = "Test Author",
            Slug = "original-title",
            CreatedAt = DateTime.UtcNow
        };

        _context.Contents.Add(content);
        await _context.SaveChangesAsync();

        content.Title = "Updated Title";
        content.Body = "Updated Body";
        content.Status = ContentStatus.Published;
        content.UpdatedAt = DateTime.UtcNow;

        var result = await _repository.UpdateAsync(content, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Title");
        result.Body.Should().Be("Updated Body");
        result.Status.Should().Be(ContentStatus.Published);
        result.UpdatedAt.Should().NotBeNull();

        var updatedContent = await _context.Contents.FindAsync(content.Id);
        updatedContent!.Title.Should().Be("Updated Title");
        updatedContent.Status.Should().Be(ContentStatus.Published);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteContent_WhenContentExists()
    {

        var content = new Content
        {
            Title = "Content to Delete",
            Body = "Body to Delete",
            Type = ContentType.Article,
            Status = ContentStatus.Draft,
            AuthorId = Guid.NewGuid(),
            AuthorName = "Test Author",
            Slug = "content-to-delete",
            CreatedAt = DateTime.UtcNow
        };

        _context.Contents.Add(content);
        await _context.SaveChangesAsync();

        var result = await _repository.DeleteAsync(content.Id, CancellationToken.None);

        result.Should().BeTrue();

        var deletedContent = await _context.Contents.FindAsync(content.Id);
        deletedContent.Should().NotBeNull();
        deletedContent!.IsDeleted.Should().BeTrue();
        deletedContent.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenContentDoesNotExist()
    {

        var nonExistentId = Guid.NewGuid();

        var result = await _repository.DeleteAsync(nonExistentId, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SlugExistsAsync_ShouldReturnTrue_WhenSlugExists()
    {

        var content = new Content
        {
            Title = "Test Content",
            Body = "Test Body",
            Type = ContentType.Article,
            Status = ContentStatus.Draft,
            AuthorId = Guid.NewGuid(),
            AuthorName = "Test Author",
            Slug = "existing-slug",
            CreatedAt = DateTime.UtcNow
        };

        _context.Contents.Add(content);
        await _context.SaveChangesAsync();

        var result = await _repository.SlugExistsAsync("existing-slug", CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task SlugExistsAsync_ShouldReturnFalse_WhenSlugDoesNotExist()
    {

        var result = await _repository.SlugExistsAsync("non-existent-slug", CancellationToken.None);

        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }
}
