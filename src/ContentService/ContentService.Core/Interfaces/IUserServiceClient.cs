namespace ContentService.Core.Interfaces;

public interface IUserServiceClient
{
    Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default);
}

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email
);
