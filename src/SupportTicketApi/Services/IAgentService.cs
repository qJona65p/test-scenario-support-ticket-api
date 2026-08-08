using SupportTicketApi.Dtos;

namespace SupportTicketApi.Services;

public interface IAgentService
{
    Task<IReadOnlyList<AgentResponse>> ListAsync(CancellationToken cancellationToken);

    Task<AgentWorkloadResponse> GetWorkloadAsync(int agentId, CancellationToken cancellationToken);
}
