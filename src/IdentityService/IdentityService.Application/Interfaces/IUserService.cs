using IdentityService.Core.DTOs.User;
using IdentityService.Core.Entities;

namespace IdentityService.Application.Interfaces;

public interface IUserService
{
    Task<UserResponse?> GetUserByIdAsync(string userId);
    Task<UserResponse?> GetUserByEmailAsync(string email);
    Task<UserResponse> UpdateUserProfileAsync(string userId, UpdateUserProfileRequest request);
    Task<bool> DeleteUserAsync(string userId);
    Task<IEnumerable<UserResponse>> GetUsersAsync(int skip = 0, int take = 50);
    Task<bool> IsEmailTakenAsync(string email, string? excludeUserId = null);
    Task<bool> IsUsernameTakenAsync(string username, string? excludeUserId = null);
}
