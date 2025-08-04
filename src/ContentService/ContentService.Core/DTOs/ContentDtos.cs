using ContentService.Core.Entities;

namespace ContentService.Core.DTOs;

public record ContentDto(
    Guid Id,
    string Title,
    string Body,
    string? Summary,
    ContentStatus Status,
    ContentType Type,
    string? FeaturedImageUrl,
    string? MetaTitle,
    string? MetaDescription,
    string? Tags,
    string? Category,
    int ViewCount,
    DateTime? PublishedAt,
    Guid AuthorId,
    string AuthorName,
    string? Slug,
    int SortOrder,
    bool IsFeatured,
    bool AllowComments,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateContentDto(
    string Title,
    string Body,
    string? Summary,
    ContentType Type,
    string? FeaturedImageUrl,
    string? MetaTitle,
    string? MetaDescription,
    string? Tags,
    string? Category,
    Guid AuthorId,
    string? Slug,
    int SortOrder,
    bool IsFeatured,
    bool AllowComments
);

public record UpdateContentDto(
    string Title,
    string Body,
    string? Summary,
    ContentStatus Status,
    ContentType Type,
    string? FeaturedImageUrl,
    string? MetaTitle,
    string? MetaDescription,
    string? Tags,
    string? Category,
    string? Slug,
    int SortOrder,
    bool IsFeatured,
    bool AllowComments
);

public record ContentSearchDto(
    string? SearchTerm,
    ContentStatus? Status,
    ContentType? Type,
    string? Category,
    Guid? AuthorId,
    bool? IsFeatured,
    int Page = 1,
    int PageSize = 10
);

public record PublishContentDto(
    DateTime? PublishAt
);
