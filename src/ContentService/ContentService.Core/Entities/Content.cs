namespace ContentService.Core.Entities;

public class Content : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public ContentType Type { get; set; } = ContentType.Article;
    public string? FeaturedImageUrl { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Tags { get; set; }
    public string? Category { get; set; }
    public int ViewCount { get; set; } = 0;
    public DateTime? PublishedAt { get; set; }

    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;

    public string? Slug { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsFeatured { get; set; } = false;
    public bool AllowComments { get; set; } = true;
}

public enum ContentStatus
{
    Draft = 1,
    Published = 2,
    Archived = 3,
    Scheduled = 4
}

public enum ContentType
{
    Article = 1,
    Page = 2,
    BlogPost = 3,
    News = 4,
    Event = 5
}
