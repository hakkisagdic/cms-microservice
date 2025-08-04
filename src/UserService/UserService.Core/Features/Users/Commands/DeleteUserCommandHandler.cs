using MediatR;
using UserService.Core.Common;
using UserService.Core.Interfaces;

namespace UserService.Core.Features.Users.Commands;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null)
        {
            return Result.Failure("User not found.");
        }

        var deleted = await _userRepository.DeleteAsync(request.Id, cancellationToken);
        if (!deleted)
        {
            return Result.Failure("Failed to delete user.");
        }

        return Result.Success();
    }
}
