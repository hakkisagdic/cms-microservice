using MediatR;
using ContentService.Core.Common;
using ContentService.Core.DTOs;
using ContentService.Core.Interfaces;

namespace ContentService.Core.Features.Contents.Queries;

public class GetAllContentsQueryHandler : IRequestHandler<GetAllContentsQuery, Result<IEnumerable<ContentDto>>>
{
    private readonly IContentRepository _contentRepository;

    public GetAllContentsQueryHandler(IContentRepository contentRepository)
    {
        _contentRepository = contentRepository;
    }

    public async Task<Result<IEnumerable<ContentDto>>> Handle(GetAllContentsQuery request, CancellationToken cancellationToken)
    {
        var contents = await _contentRepository.GetAllAsync(cancellationToken);

        var contentDtos = contents.Select(content => new ContentDto(
            content.Id,
            content.Title,
            content.Body,
            content.Summary,
            content.Status,
            content.Type,
            content.FeaturedImageUrl,
            content.MetaTitle,
            content.MetaDescription,
            content.Tags,
            content.Category,
            content.ViewCount,
            content.PublishedAt,
            content.AuthorId,
            content.AuthorName,
            content.Slug,
            content.SortOrder,
            content.IsFeatured,
            content.AllowComments,
            content.CreatedAt,
            content.UpdatedAt
        ));

        return Result<IEnumerable<ContentDto>>.Success(contentDtos);
    }
}
