using Microsoft.EntityFrameworkCore;
using SupportTicketApi.Data;
using SupportTicketApi.Dtos;
using SupportTicketApi.Models;
using SupportTicketApi.Services.Exceptions;

namespace SupportTicketApi.Services;

public class AgentService : IAgentService
{
    private readonly AppDbContext _db;

    public AgentService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<AgentResponse>> ListAsync(CancellationToken cancellationToken) =>
        await _db.Agents
            .AsNoTracking()
            .OrderBy(a => a.Id)
            .Select(a => new AgentResponse(a.Id, a.Name, a.Email, a.Team, a.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<AgentWorkloadResponse> GetWorkloadAsync(int agentId, CancellationToken cancellationToken)
    {
        var agent = await _db.Agents
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == agentId, cancellationToken);

        if (agent is null)
        {
            throw new NotFoundException($"Agent {agentId} was not found.");
        }

        var openCount = await _db.Tickets.CountAsync(
            t => t.AssignedAgentId == agentId
                 && (t.Status == TicketStatus.New || t.Status == TicketStatus.Assigned),
            cancellationToken);

        return new AgentWorkloadResponse(agent.Id, agent.Name, openCount);
    }
}
