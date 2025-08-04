using MediatR;
using UserService.Core.Common;

namespace UserService.Core.Features.Users.Commands;

public record DeleteUserCommand(Guid Id) : IRequest<Result>;
