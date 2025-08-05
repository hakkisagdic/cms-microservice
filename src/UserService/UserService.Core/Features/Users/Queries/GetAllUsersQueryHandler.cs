using MediatR;
using UserService.Core.Common;
using UserService.Core.DTOs;
using UserService.Core.Interfaces;

namespace UserService.Core.Features.Users.Queries;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<IEnumerable<UserDto>>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<IEnumerable<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        var userDtos = users.Select(user => new UserDto(
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
        ));

        return Result<IEnumerable<UserDto>>.Success(userDtos);
    }
}
