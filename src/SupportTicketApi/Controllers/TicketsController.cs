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
