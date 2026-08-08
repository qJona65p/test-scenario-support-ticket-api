namespace SupportTicketApi.Models;

public class Comment
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public string AuthorName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    /// <summary>Internal notes are visible to agents only, never to the requester.</summary>
    public bool IsInternal { get; set; }

    public DateTime CreatedUtc { get; set; }
}
