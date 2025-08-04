using Microsoft.EntityFrameworkCore;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(UserDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetByStatusAsync(UserStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(u => u.Status == status).ToListAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllAsync(cancellationToken);
        }

        searchTerm = searchTerm.ToLower();
        
        return await _dbSet
            .Where(u => u.FirstName.ToLower().Contains(searchTerm) ||
                       u.LastName.ToLower().Contains(searchTerm) ||
                       u.Email.ToLower().Contains(searchTerm) ||
                       (u.Department != null && u.Department.ToLower().Contains(searchTerm)) ||
                       (u.Position != null && u.Position.ToLower().Contains(searchTerm)))
            .ToListAsync(cancellationToken);
    }
}
