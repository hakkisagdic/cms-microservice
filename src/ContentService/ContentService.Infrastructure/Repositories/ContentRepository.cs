using Microsoft.EntityFrameworkCore;
using ContentService.Core.Entities;
using ContentService.Core.Interfaces;
using ContentService.Infrastructure.Data;

namespace ContentService.Infrastructure.Repositories;

public class ContentRepository : Repository<Content>, IContentRepository
{
    public ContentRepository(ContentDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Content>> GetByStatusAsync(ContentStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(c => c.Status == status).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Content>> GetByAuthorIdAsync(Guid authorId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(c => c.AuthorId == authorId).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Content>> GetByTypeAsync(ContentType type, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(c => c.Type == type).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Content>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(c => c.Category == category).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Content>> GetFeaturedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(c => c.IsFeatured && c.Status == ContentStatus.Published)
            .OrderByDescending(c => c.PublishedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Content>> SearchContentsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllAsync(cancellationToken);
        }

        searchTerm = searchTerm.ToLower();

        return await _dbSet
            .Where(c => c.Title.ToLower().Contains(searchTerm) ||
                       c.Body.ToLower().Contains(searchTerm) ||
                       (c.Summary != null && c.Summary.ToLower().Contains(searchTerm)) ||
                       (c.Tags != null && c.Tags.ToLower().Contains(searchTerm)) ||
                       (c.Category != null && c.Category.ToLower().Contains(searchTerm)))
            .ToListAsync(cancellationToken);
    }

    public async Task<Content?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.Slug == slug, cancellationToken);
    }

    public async Task<IEnumerable<Content>> GetPublishedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.Status == ContentStatus.Published)
            .OrderByDescending(c => c.PublishedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task IncrementViewCountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var content = await GetByIdAsync(id, cancellationToken);
        if (content != null)
        {
            content.ViewCount++;
            await UpdateAsync(content, cancellationToken);
        }
    }
}
