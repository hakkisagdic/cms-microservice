using MediatR;
using ContentService.Core.Common;

namespace ContentService.Core.Features.Contents.Commands;

public record DeleteContentCommand(Guid Id) : IRequest<Result>;
