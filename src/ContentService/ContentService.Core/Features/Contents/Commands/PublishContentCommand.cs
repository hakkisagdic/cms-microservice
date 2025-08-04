using MediatR;
using ContentService.Core.Common;
using ContentService.Core.DTOs;

namespace ContentService.Core.Features.Contents.Commands;

public record PublishContentCommand(Guid Id, PublishContentDto PublishData) : IRequest<Result<ContentDto>>;
