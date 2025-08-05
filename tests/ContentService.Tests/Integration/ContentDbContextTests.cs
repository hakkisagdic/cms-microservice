using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;
using ContentService.Infrastructure.Data;
using ContentService.Core.Entities;

namespace ContentService.Tests.Integration;

public class ContentDbContextTests : IDisposable
{
    private readonly ContentDbContext _context;

    public ContentDbContextTests()
    {
        var options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContentDbContext(options);
    }

    [Fact]
    public void ContentDbContext_ShouldHaveContentsDbSet()
    {

        _context.Contents.Should().NotBeNull();
    }

    [Fact]
    public void ContentEntity_ShouldHaveCorrectConfiguration()
    {

        var entityType = _context.Model.FindEntityType(typeof(Content));

        entityType.Should().NotBeNull();

        var primaryKey = entityType!.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().HaveCount(1);
        primaryKey.Properties[0].Name.Should().Be("Id");

        var titleProperty = entityType.FindProperty("Title");
        titleProperty.Should().NotBeNull();
        titleProperty!.IsNullable.Should().BeFalse();
        titleProperty.GetMaxLength().Should().Be(200);

        var bodyProperty = entityType.FindProperty("Body");
        bodyProperty.Should().NotBeNull();
        bodyProperty!.IsNullable.Should().BeFalse();

        var authorNameProperty = entityType.FindProperty("AuthorName");
        authorNameProperty.Should().NotBeNull();
        authorNameProperty!.IsNullable.Should().BeFalse();
        authorNameProperty.GetMaxLength().Should().Be(100);
    }

    [Fact]
    public void ContentEntity_ShouldHaveCorrectIndexes()
    {

        var entityType = _context.Model.FindEntityType(typeof(Content));

        entityType.Should().NotBeNull();

        var indexes = entityType!.GetIndexes().ToList();
        indexes.Should().NotBeEmpty();

        var slugIndex = indexes.FirstOrDefault(i => i.Properties.Any(p => p.Name == "Slug"));
        slugIndex.Should().NotBeNull();
        slugIndex!.IsUnique.Should().BeTrue();

        var authorIdIndex = indexes.FirstOrDefault(i => i.Properties.Any(p => p.Name == "AuthorId"));
        authorIdIndex.Should().NotBeNull();

        var statusIndex = indexes.FirstOrDefault(i => i.Properties.Any(p => p.Name == "Status"));
        statusIndex.Should().NotBeNull();
    }

    [Fact]
    public void ContentEntity_ShouldHaveDefaultValues()
    {

        var entityType = _context.Model.FindEntityType(typeof(Content));

        entityType.Should().NotBeNull();

        var isDeletedProperty = entityType!.FindProperty("IsDeleted");
        isDeletedProperty.Should().NotBeNull();
        isDeletedProperty!.GetDefaultValue().Should().Be(false);

        var viewCountProperty = entityType.FindProperty("ViewCount");
        viewCountProperty.Should().NotBeNull();
        viewCountProperty!.GetDefaultValue().Should().Be(0);

        var isFeaturedProperty = entityType.FindProperty("IsFeatured");
        isFeaturedProperty.Should().NotBeNull();
        isFeaturedProperty!.GetDefaultValue().Should().Be(false);

        var allowCommentsProperty = entityType.FindProperty("AllowComments");
        allowCommentsProperty.Should().NotBeNull();
        allowCommentsProperty!.GetDefaultValue().Should().Be(true);
    }

    [Fact]
    public void ContentEntity_ShouldHaveQueryFilterForSoftDelete()
    {

        var entityType = _context.Model.FindEntityType(typeof(Content));

        entityType.Should().NotBeNull();

        var queryFilter = entityType!.GetQueryFilter();
        queryFilter.Should().NotBeNull();
    }

    [Fact]
    public async Task ContentDbContext_ShouldApplySoftDeleteFilter()
    {

        var content1 = new Content
        {
            Id = Guid.NewGuid(),
            Title = "Test Content 1",
            Body = "Test Body 1",
            AuthorId = Guid.NewGuid(),
            AuthorName = "Test Author",
            Status = ContentStatus.Published,
            Type = ContentType.Article,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var content2 = new Content
        {
            Id = Guid.NewGuid(),
            Title = "Test Content 2",
            Body = "Test Body 2",
            AuthorId = Guid.NewGuid(),
            AuthorName = "Test Author",
            Status = ContentStatus.Published,
            Type = ContentType.Article,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = true
        };

        _context.Contents.AddRange(content1, content2);
        await _context.SaveChangesAsync();

        var activeContents = await _context.Contents.ToListAsync();

        activeContents.Should().HaveCount(1);
        activeContents[0].Id.Should().Be(content1.Id);
    }

    [Fact]
    public async Task ContentDbContext_InMemoryDatabase_AllowsDuplicateSlugs()
    {

        var content1 = new Content
        {
            Id = Guid.NewGuid(),
            Title = "Test Content 1",
            Body = "Test body 1",
            Slug = "duplicate-slug",
            AuthorId = Guid.NewGuid(),
            Status = ContentStatus.Draft,
            Type = ContentType.Article,
            CreatedAt = DateTime.UtcNow
        };

        var content2 = new Content
        {
            Id = Guid.NewGuid(),
            Title = "Test Content 2",
            Body = "Test body 2",
            Slug = "duplicate-slug",
            AuthorId = Guid.NewGuid(),
            Status = ContentStatus.Draft,
            Type = ContentType.Article,
            CreatedAt = DateTime.UtcNow
        };

        _context.Contents.Add(content1);
        await _context.SaveChangesAsync();

        _context.Contents.Add(content2);

        var saveResult = await _context.SaveChangesAsync();
        saveResult.Should().BeGreaterThan(0);

        var contentsWithSameSlug = await _context.Contents
            .Where(c => c.Slug == "duplicate-slug")
            .CountAsync();
        contentsWithSameSlug.Should().Be(2);
    }

    [Fact]
    public void ContentEntity_ShouldHaveProperConfiguration()
    {

        var entityType = _context.Model.FindEntityType(typeof(Content));

        entityType.Should().NotBeNull();

        var statusProperty = entityType!.FindProperty("Status");
        statusProperty.Should().NotBeNull();

        var typeProperty = entityType.FindProperty("Type");
        typeProperty.Should().NotBeNull();

        statusProperty!.ClrType.Should().Be(typeof(ContentStatus));
        typeProperty!.ClrType.Should().Be(typeof(ContentType));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
