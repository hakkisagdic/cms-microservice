using MediatR;
using UserService.Core.Common;
using UserService.Core.DTOs;
using UserService.Core.Interfaces;

namespace UserService.Core.Features.Users.Queries;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto?>>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto?>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user == null)
        {
            return Result<UserDto?>.Failure("User not found.");
        }

        var userDto = new UserDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber,
            user.DateOfBirth,
            user.Status,
            user.ProfileImageUrl,
            user.Bio,
            user.Department,
            user.Position,
            user.CreatedAt,
            user.UpdatedAt
        );

        return Result<UserDto?>.Success(userDto);
    }
}
