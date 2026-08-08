namespace SupportTicketApi.Models;

public class Ticket
{
    public int Id { get; set; }

    /// <summary>Human-facing identifier in the form TKT-0001.</summary>
    public string Reference { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; }
    public string RequesterEmail { get; set; } = string.Empty;

    public int? AssignedAgentId { get; set; }
    public Agent? AssignedAgent { get; set; }

    public DateTime CreatedUtc { get; set; }
    public DateTime? ResolvedUtc { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
