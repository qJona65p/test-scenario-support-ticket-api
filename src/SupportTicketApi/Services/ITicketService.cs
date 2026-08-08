using SupportTicketApi.Dtos;

namespace SupportTicketApi.Services;

public interface ITicketService
{
    Task<PagedResult<TicketSummaryResponse>> GetQueueAsync(
        TicketQueueRequest request, CancellationToken cancellationToken);

    Task<TicketDetailResponse> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<TicketDetailResponse> CreateAsync(
        CreateTicketRequest request, CancellationToken cancellationToken);

    Task<TicketDetailResponse> ResolveAsync(int id, CancellationToken cancellationToken);
}
