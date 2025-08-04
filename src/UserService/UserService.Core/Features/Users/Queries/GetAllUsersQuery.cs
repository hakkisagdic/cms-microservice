using MediatR;
using UserService.Core.Common;
using UserService.Core.DTOs;

namespace UserService.Core.Features.Users.Queries;

public record GetAllUsersQuery() : IRequest<Result<IEnumerable<UserDto>>>;
