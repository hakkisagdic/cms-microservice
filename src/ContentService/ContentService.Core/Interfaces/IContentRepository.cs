using ContentService.Core.Entities;

namespace ContentService.Core.Interfaces;

public interface IContentRepository : IRepository<Content>
{
    Task<IEnumerable<Content>> GetByStatusAsync(ContentStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Content>> GetByAuthorIdAsync(Guid authorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Content>> GetByTypeAsync(ContentType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<Content>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<IEnumerable<Content>> GetFeaturedAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Content>> SearchContentsAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<Content?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Content>> GetPublishedAsync(CancellationToken cancellationToken = default);
    Task IncrementViewCountAsync(Guid id, CancellationToken cancellationToken = default);
}
