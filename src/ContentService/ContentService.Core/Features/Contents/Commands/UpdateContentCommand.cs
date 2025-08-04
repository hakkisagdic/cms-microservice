using MediatR;
using ContentService.Core.Common;
using ContentService.Core.DTOs;

namespace ContentService.Core.Features.Contents.Commands;

public record UpdateContentCommand(Guid Id, UpdateContentDto Content) : IRequest<Result<ContentDto>>;
