namespace SupportTicketApi.Dtos;

public record AgentResponse(
    int Id,
    string Name,
    string Email,
    string Team,
    bool IsActive);

/// <param name="OpenTicketCount">Tickets currently New or Assigned to this agent.</param>
public record AgentWorkloadResponse(
    int AgentId,
    string Name,
    int OpenTicketCount);
