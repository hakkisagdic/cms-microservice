using MediatR;
using ContentService.Core.Common;
using ContentService.Core.DTOs;
using ContentService.Core.Entities;
using ContentService.Core.Interfaces;

namespace ContentService.Core.Features.Contents.Commands;

public class CreateContentCommandHandler : IRequestHandler<CreateContentCommand, Result<ContentDto>>
{
    private readonly IContentRepository _contentRepository;
    private readonly IUserServiceClient _userServiceClient;

    public CreateContentCommandHandler(IContentRepository contentRepository, IUserServiceClient userServiceClient)
    {
        _contentRepository = contentRepository;
        _userServiceClient = userServiceClient;
    }

    public async Task<Result<ContentDto>> Handle(CreateContentCommand request, CancellationToken cancellationToken)
    {

        var authorExists = await _userServiceClient.UserExistsAsync(request.Content.AuthorId, cancellationToken);
        if (!authorExists)
        {
            return Result<ContentDto>.Failure("Author not found.");
        }

        var author = await _userServiceClient.GetUserByIdAsync(request.Content.AuthorId, cancellationToken);
        if (author == null)
        {
            return Result<ContentDto>.Failure("Unable to retrieve author information.");
        }

        var slug = request.Content.Slug ?? GenerateSlug(request.Content.Title);

        if (await _contentRepository.SlugExistsAsync(slug, cancellationToken))
        {
            slug = await GenerateUniqueSlug(slug, cancellationToken);
        }

        var content = new Content
        {
            Title = request.Content.Title,
            Body = request.Content.Body,
            Summary = request.Content.Summary,
            Type = request.Content.Type,
            FeaturedImageUrl = request.Content.FeaturedImageUrl,
            MetaTitle = request.Content.MetaTitle,
            MetaDescription = request.Content.MetaDescription,
            Tags = request.Content.Tags,
            Category = request.Content.Category,
            AuthorId = request.Content.AuthorId,
            AuthorName = $"{author.FirstName} {author.LastName}",
            Slug = slug,
            SortOrder = request.Content.SortOrder,
            IsFeatured = request.Content.IsFeatured,
            AllowComments = request.Content.AllowComments,
            Status = ContentStatus.Draft
        };

        var createdContent = await _contentRepository.AddAsync(content, cancellationToken);

        var contentDto = MapToDto(createdContent);
        return Result<ContentDto>.Success(contentDto);
    }

    private string GenerateSlug(string title)
    {
        return title.ToLower()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace("?", "")
            .Replace("!", "")
            .Replace(".", "")
            .Replace(",", "");
    }

    private async Task<string> GenerateUniqueSlug(string baseSlug, CancellationToken cancellationToken)
    {
        var counter = 1;
        var uniqueSlug = $"{baseSlug}-{counter}";

        while (await _contentRepository.SlugExistsAsync(uniqueSlug, cancellationToken))
        {
            counter++;
            uniqueSlug = $"{baseSlug}-{counter}";
        }

        return uniqueSlug;
    }

    private static ContentDto MapToDto(Content content)
    {
        return new ContentDto(
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
    }
}
