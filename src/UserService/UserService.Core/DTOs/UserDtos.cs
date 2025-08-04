using UserService.Core.Entities;

namespace UserService.Core.DTOs;

public record UserDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    UserStatus Status,
    string? ProfileImageUrl,
    string? Bio,
    string? Department,
    string? Position,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateUserDto(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    string? ProfileImageUrl,
    string? Bio,
    string? Department,
    string? Position
);

public record UpdateUserDto(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    UserStatus Status,
    string? ProfileImageUrl,
    string? Bio,
    string? Department,
    string? Position
);

public record UserSearchDto(
    string? SearchTerm,
    UserStatus? Status,
    int Page = 1,
    int PageSize = 10
);
