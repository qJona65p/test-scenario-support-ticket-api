using Microsoft.EntityFrameworkCore;
using SupportTicketApi.Data;
using SupportTicketApi.Dtos;
using SupportTicketApi.Models;
using SupportTicketApi.Services.Exceptions;

namespace SupportTicketApi.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _clock;
    private readonly ILogger<TicketService> _logger;

    public TicketService(AppDbContext db, TimeProvider clock, ILogger<TicketService> logger)
    {
        _db = db;
        _clock = clock;
        _logger = logger;
    }

    public async Task<PagedResult<TicketSummaryResponse>> GetQueueAsync(
        TicketQueueRequest request, CancellationToken cancellationToken)
    {
        var query = _db.Tickets
            .AsNoTracking()
            .Where(t => t.Status == TicketStatus.New || t.Status == TicketStatus.Assigned);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TicketSummaryResponse(
                t.Id,
                t.Reference,
                t.Subject,
                t.Priority.ToString(),
                t.Status.ToString(),
                t.AssignedAgentId,
                t.CreatedUtc))
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Queue returned {Count} of {Total}", items.Count, totalCount);

        return new PagedResult<TicketSummaryResponse>(items, request.Page, request.PageSize, totalCount);
    }

    public async Task<TicketDetailResponse> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var ticket = await _db.Tickets
            .AsNoTracking()
            .Include(t => t.AssignedAgent)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (ticket is null)
        {
            throw new NotFoundException($"Ticket {id} was not found.");
        }

        return Map(ticket);
    }

    public async Task<TicketDetailResponse> CreateAsync(
        CreateTicketRequest request, CancellationToken cancellationToken)
    {
        var ticket = new Ticket
        {
            Reference = string.Empty,
            Subject = request.Subject,
            Description = request.Description,
            Priority = request.Priority,
            Status = TicketStatus.New,
            RequesterEmail = request.RequesterEmail,
            CreatedUtc = _clock.GetUtcNow().UtcDateTime
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync(cancellationToken);

        // The reference embeds the identity value, so it is stamped after the insert.
        ticket.Reference = $"TKT-{ticket.Id:D4}";
        await _db.SaveChangesAsync(cancellationToken);

        return Map(ticket);
    }

    public async Task<TicketDetailResponse> ResolveAsync(int id, CancellationToken cancellationToken)
    {
        var ticket = await _db.Tickets
            .Include(t => t.AssignedAgent)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (ticket is null)
        {
            throw new NotFoundException($"Ticket {id} was not found.");
        }

        if (ticket.Status is TicketStatus.Resolved or TicketStatus.Closed)
        {
            throw new ConflictException($"Ticket {ticket.Reference} is already {ticket.Status}.");
        }

        ticket.Status = TicketStatus.Resolved;
        ticket.ResolvedUtc = _clock.GetUtcNow().UtcDateTime;
        await _db.SaveChangesAsync(cancellationToken);

        return Map(ticket);
    }

    private static TicketDetailResponse Map(Ticket ticket) => new(
        ticket.Id,
        ticket.Reference,
        ticket.Subject,
        ticket.Description,
        ticket.Priority.ToString(),
        ticket.Status.ToString(),
        ticket.RequesterEmail,
        ticket.AssignedAgentId,
        ticket.AssignedAgent?.Name,
        ticket.CreatedUtc,
        ticket.ResolvedUtc);
}
