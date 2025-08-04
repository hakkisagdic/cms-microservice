using MediatR;
using UserService.Core.Common;
using UserService.Core.DTOs;

namespace UserService.Core.Features.Users.Commands;

public record CreateUserCommand(CreateUserDto User) : IRequest<Result<UserDto>>;
