namespace SupportTicketApi.Models;

/// <summary>New and Assigned are "open". Resolved and Closed are not.</summary>
public enum TicketStatus
{
    New = 0,
    Assigned = 1,
    Resolved = 2,
    Closed = 3
}
