using MediatR;
using ContentService.Core.Common;
using ContentService.Core.DTOs;

namespace ContentService.Core.Features.Contents.Queries;

public record GetContentByIdQuery(Guid Id) : IRequest<Result<ContentDto?>>;
