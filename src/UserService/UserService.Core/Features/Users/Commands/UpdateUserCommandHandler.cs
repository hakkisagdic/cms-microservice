using MediatR;
using UserService.Core.Common;
using UserService.Core.DTOs;
using UserService.Core.Interfaces;

namespace UserService.Core.Features.Users.Commands;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (existingUser == null)
        {
            return Result<UserDto>.Failure("User not found.");
        }

        // Check if email is being changed and if the new email already exists
        if (existingUser.Email != request.User.Email && await _userRepository.EmailExistsAsync(request.User.Email, cancellationToken))
        {
            return Result<UserDto>.Failure("A user with this email already exists.");
        }

        existingUser.FirstName = request.User.FirstName;
        existingUser.LastName = request.User.LastName;
        existingUser.Email = request.User.Email;
        existingUser.PhoneNumber = request.User.PhoneNumber;
        existingUser.DateOfBirth = request.User.DateOfBirth;
        existingUser.Status = request.User.Status;
        existingUser.ProfileImageUrl = request.User.ProfileImageUrl;
        existingUser.Bio = request.User.Bio;
        existingUser.Department = request.User.Department;
        existingUser.Position = request.User.Position;
        existingUser.UpdatedAt = DateTime.UtcNow;

        var updatedUser = await _userRepository.UpdateAsync(existingUser, cancellationToken);

        var userDto = new UserDto(
            updatedUser.Id,
            updatedUser.FirstName,
            updatedUser.LastName,
            updatedUser.Email,
            updatedUser.PhoneNumber,
            updatedUser.DateOfBirth,
            updatedUser.Status,
            updatedUser.ProfileImageUrl,
            updatedUser.Bio,
            updatedUser.Department,
            updatedUser.Position,
            updatedUser.CreatedAt,
            updatedUser.UpdatedAt
        );

        return Result<UserDto>.Success(userDto);
    }
}
