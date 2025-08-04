using MediatR;
using ContentService.Core.Common;
using ContentService.Core.DTOs;

namespace ContentService.Core.Features.Contents.Commands;

public record CreateContentCommand(CreateContentDto Content) : IRequest<Result<ContentDto>>;
