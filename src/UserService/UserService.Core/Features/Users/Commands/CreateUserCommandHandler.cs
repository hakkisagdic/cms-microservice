using MediatR;
using UserService.Core.Common;
using UserService.Core.DTOs;
using UserService.Core.Entities;
using UserService.Core.Interfaces;

namespace UserService.Core.Features.Users.Commands;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(request.User.Email, cancellationToken))
        {
            return Result<UserDto>.Failure("A user with this email already exists.");
        }

        var user = new User
        {
            FirstName = request.User.FirstName,
            LastName = request.User.LastName,
            Email = request.User.Email,
            PhoneNumber = request.User.PhoneNumber,
            DateOfBirth = request.User.DateOfBirth,
            ProfileImageUrl = request.User.ProfileImageUrl,
            Bio = request.User.Bio,
            Department = request.User.Department,
            Position = request.User.Position,
            Status = UserStatus.Active
        };

        var createdUser = await _userRepository.AddAsync(user, cancellationToken);

        var userDto = new UserDto(
            createdUser.Id,
            createdUser.FirstName,
            createdUser.LastName,
            createdUser.Email,
            createdUser.PhoneNumber,
            createdUser.DateOfBirth,
            createdUser.Status,
            createdUser.ProfileImageUrl,
            createdUser.Bio,
            createdUser.Department,
            createdUser.Position,
            createdUser.CreatedAt,
            createdUser.UpdatedAt
        );

        return Result<UserDto>.Success(userDto);
    }
}
