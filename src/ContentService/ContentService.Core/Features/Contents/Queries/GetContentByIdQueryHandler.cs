using MediatR;
using ContentService.Core.Common;
using ContentService.Core.DTOs;
using ContentService.Core.Interfaces;

namespace ContentService.Core.Features.Contents.Queries;

public class GetContentByIdQueryHandler : IRequestHandler<GetContentByIdQuery, Result<ContentDto?>>
{
    private readonly IContentRepository _contentRepository;

    public GetContentByIdQueryHandler(IContentRepository contentRepository)
    {
        _contentRepository = contentRepository;
    }

    public async Task<Result<ContentDto?>> Handle(GetContentByIdQuery request, CancellationToken cancellationToken)
    {
        var content = await _contentRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (content == null)
        {
            return Result<ContentDto?>.Failure("Content not found.");
        }

        var contentDto = new ContentDto(
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
        );

        return Result<ContentDto?>.Success(contentDto);
    }
}
