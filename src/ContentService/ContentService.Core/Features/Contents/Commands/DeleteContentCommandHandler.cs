using MediatR;
using ContentService.Core.Common;
using ContentService.Core.Interfaces;

namespace ContentService.Core.Features.Contents.Commands;

public class DeleteContentCommandHandler : IRequestHandler<DeleteContentCommand, Result>
{
    private readonly IContentRepository _contentRepository;

    public DeleteContentCommandHandler(IContentRepository contentRepository)
    {
        _contentRepository = contentRepository;
    }

    public async Task<Result> Handle(DeleteContentCommand request, CancellationToken cancellationToken)
    {
        var content = await _contentRepository.GetByIdAsync(request.Id);
        if (content == null)
        {
            return Result.Failure("Content not found.");
        }

        await _contentRepository.DeleteAsync(request.Id);

        return Result.Success();
    }
}
