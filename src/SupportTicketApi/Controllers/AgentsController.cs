using Microsoft.AspNetCore.Mvc;
using SupportTicketApi.Dtos;
using SupportTicketApi.Services;

namespace SupportTicketApi.Controllers;

[ApiController]
[Route("api/agents")]
[Produces("application/json")]
public class AgentsController : ControllerBase
{
    private readonly IAgentService _agents;

    public AgentsController(IAgentService agents) => _agents = agents;

    /// <summary>Lists every support agent, active or not.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AgentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AgentResponse>>> List(CancellationToken cancellationToken)
        => Ok(await _agents.ListAsync(cancellationToken));

    /// <summary>Counts the tickets currently open against one agent.</summary>
    [HttpGet("{id:int}/workload")]
    [ProducesResponseType(typeof(AgentWorkloadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgentWorkloadResponse>> Workload(int id, CancellationToken cancellationToken)
        => Ok(await _agents.GetWorkloadAsync(id, cancellationToken));
}
