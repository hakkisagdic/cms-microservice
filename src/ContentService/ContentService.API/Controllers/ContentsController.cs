using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ContentService.Core.DTOs;
using ContentService.Core.Features.Contents.Commands;
using ContentService.Core.Features.Contents.Queries;

namespace ContentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ContentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ContentsController> _logger;

    public ContentsController(IMediator mediator, ILogger<ContentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all contents
    /// </summary>
    /// <returns>List of contents</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ContentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ContentDto>>> GetAllContents(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetAllContentsQuery(), cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all contents");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Get content by ID
    /// </summary>
    /// <param name="id">Content ID</param>
    /// <returns>Content details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ContentDto>> GetContentById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetContentByIdQuery(id), cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return NotFound(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting content with ID {ContentId}", id);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Create a new content
    /// </summary>
    /// <param name="createContentDto">Content creation data</param>
    /// <returns>Created content</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ContentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ContentDto>> CreateContent([FromBody] CreateContentDto createContentDto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new CreateContentCommand(createContentDto), cancellationToken);

            if (result.IsSuccess)
            {
                return CreatedAtAction(
                    nameof(GetContentById),
                    new { id = result.Value!.Id },
                    result.Value);
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating content");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Update an existing content
    /// </summary>
    /// <param name="id">Content ID</param>
    /// <param name="updateContentDto">Content update data</param>
    /// <returns>Updated content</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ContentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ContentDto>> UpdateContent(Guid id, [FromBody] UpdateContentDto updateContentDto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new UpdateContentCommand(id, updateContentDto), cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating content with ID {ContentId}", id);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Delete a content
    /// </summary>
    /// <param name="id">Content ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteContent(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new DeleteContentCommand(id), cancellationToken);

            if (result.IsSuccess)
            {
                return NoContent();
            }

            return NotFound(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting content with ID {ContentId}", id);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Publish a content
    /// </summary>
    /// <param name="id">Content ID</param>
    /// <param name="publishDto">Publish data</param>
    /// <returns>Published content</returns>
    [HttpPost("{id:guid}/publish")]
    [ProducesResponseType(typeof(ContentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ContentDto>> PublishContent(Guid id, [FromBody] PublishContentDto publishDto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new PublishContentCommand(id, publishDto), cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while publishing content with ID {ContentId}", id);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
}
