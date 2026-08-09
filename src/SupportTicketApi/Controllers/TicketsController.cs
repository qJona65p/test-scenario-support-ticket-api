using Microsoft.AspNetCore.Mvc;
using SupportTicketApi.Dtos;
using SupportTicketApi.Services;

namespace SupportTicketApi.Controllers;

[ApiController]
[Route("api/tickets")]
[Produces("application/json")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _tickets;

    public TicketsController(ITicketService tickets) => _tickets = tickets;

    /// <summary>Lists the open ticket queue, most severe first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TicketSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<TicketSummaryResponse>>> Queue(
        [FromQuery] TicketQueueRequest request, CancellationToken cancellationToken)
        => Ok(await _tickets.GetQueueAsync(request, cancellationToken));

    /// <summary>Gets a single ticket by its identifier.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TicketDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDetailResponse>> GetById(int id, CancellationToken cancellationToken)
        => Ok(await _tickets.GetByIdAsync(id, cancellationToken));

    [HttpGet("{id:int}/comments")]
    [ProducesResponseType(typeof(List<TicketCommentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<TicketCommentResponse>>> GetComments(int id, CancellationToken cancellationToken, [FromQuery] bool? includeInternal = false)
        => Ok(await _tickets.GetCommentsByIdAsync(id, cancellationToken, includeInternal));

    [HttpPost("{id:int}/comments")]
    [ProducesResponseType(typeof(TicketCommentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketCommentResponse>> AddComment(int id, [FromBody] TicketCommentRequest request, CancellationToken cancellationToken)
    {
        var created = await _tickets.PostComment(id, request, cancellationToken);
        return CreatedAtAction(nameof(GetComments), new { id = created.Id }, created);
    }

    [HttpPost("{id:int}/assign")]
    [ProducesResponseType(typeof(TicketDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TicketDetailResponse>> AssignTicket(int id, [FromBody] AssignTicketRequest request, CancellationToken cancellationToken)
        => Ok(await _tickets.AssignTicketTo(id, request.AgentId, cancellationToken));

    /// <summary>Raises a new ticket.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TicketDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketDetailResponse>> Create(
        [FromBody] CreateTicketRequest request, CancellationToken cancellationToken)
    {
        var created = await _tickets.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Marks an open ticket resolved.</summary>
    [HttpPost("{id:int}/resolve")]
    [ProducesResponseType(typeof(TicketDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TicketDetailResponse>> Resolve(int id, CancellationToken cancellationToken)
        => Ok(await _tickets.ResolveAsync(id, cancellationToken));
}
