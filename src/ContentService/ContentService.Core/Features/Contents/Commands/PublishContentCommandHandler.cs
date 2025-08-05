using MediatR;
using ContentService.Core.Common;
using ContentService.Core.DTOs;
using ContentService.Core.Interfaces;
using ContentService.Core.Entities;

namespace ContentService.Core.Features.Contents.Commands;

public class PublishContentCommandHandler : IRequestHandler<PublishContentCommand, Result<ContentDto>>
{
    private readonly IContentRepository _contentRepository;

    public PublishContentCommandHandler(IContentRepository contentRepository)
    {
        _contentRepository = contentRepository;
    }

    public async Task<Result<ContentDto>> Handle(PublishContentCommand request, CancellationToken cancellationToken)
    {
        var content = await _contentRepository.GetByIdAsync(request.Id);
        if (content == null)
        {
            return Result<ContentDto>.Failure("Content not found.");
        }

        content.Status = ContentStatus.Published;
        content.PublishedAt = request.PublishData.PublishAt ?? DateTime.UtcNow;
        content.UpdatedAt = DateTime.UtcNow;

        await _contentRepository.UpdateAsync(content);

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

        return Result<ContentDto>.Success(contentDto);
    }
}
