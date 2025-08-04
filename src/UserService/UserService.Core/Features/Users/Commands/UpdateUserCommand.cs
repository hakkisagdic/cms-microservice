using MediatR;
using UserService.Core.Common;
using UserService.Core.DTOs;

namespace UserService.Core.Features.Users.Commands;

public record UpdateUserCommand(Guid Id, UpdateUserDto User) : IRequest<Result<UserDto>>;
