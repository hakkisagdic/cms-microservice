using MediatR;
using ContentService.Core.Common;
using ContentService.Core.DTOs;
using ContentService.Core.Interfaces;
using ContentService.Core.Entities;

namespace ContentService.Core.Features.Contents.Commands;

public class UpdateContentCommandHandler : IRequestHandler<UpdateContentCommand, Result<ContentDto>>
{
    private readonly IContentRepository _contentRepository;

    public UpdateContentCommandHandler(IContentRepository contentRepository)
    {
        _contentRepository = contentRepository;
    }

    public async Task<Result<ContentDto>> Handle(UpdateContentCommand request, CancellationToken cancellationToken)
    {
        var existingContent = await _contentRepository.GetByIdAsync(request.Id);
        if (existingContent == null)
        {
            return Result<ContentDto>.Failure("Content not found.");
        }

        existingContent.Title = request.Content.Title;
        existingContent.Body = request.Content.Body;
        existingContent.Summary = request.Content.Summary;
        existingContent.Status = request.Content.Status;
        existingContent.Type = request.Content.Type;
        existingContent.FeaturedImageUrl = request.Content.FeaturedImageUrl;
        existingContent.MetaTitle = request.Content.MetaTitle;
        existingContent.MetaDescription = request.Content.MetaDescription;
        existingContent.Tags = request.Content.Tags;
        existingContent.Category = request.Content.Category;
        existingContent.Slug = request.Content.Slug;
        existingContent.SortOrder = request.Content.SortOrder;
        existingContent.IsFeatured = request.Content.IsFeatured;
        existingContent.AllowComments = request.Content.AllowComments;
        existingContent.UpdatedAt = DateTime.UtcNow;

        await _contentRepository.UpdateAsync(existingContent);

        var contentDto = new ContentDto(
            existingContent.Id,
            existingContent.Title,
            existingContent.Body,
            existingContent.Summary,
            existingContent.Status,
            existingContent.Type,
            existingContent.FeaturedImageUrl,
            existingContent.MetaTitle,
            existingContent.MetaDescription,
            existingContent.Tags,
            existingContent.Category,
            existingContent.ViewCount,
            existingContent.PublishedAt,
            existingContent.AuthorId,
            existingContent.AuthorName,
            existingContent.Slug,
            existingContent.SortOrder,
            existingContent.IsFeatured,
            existingContent.AllowComments,
            existingContent.CreatedAt,
            existingContent.UpdatedAt
        );

        return Result<ContentDto>.Success(contentDto);
    }
}
