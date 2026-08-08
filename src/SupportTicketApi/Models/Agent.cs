namespace SupportTicketApi.Models;

public class Agent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Team { get; set; } = string.Empty;

    /// <summary>Agents who have left the team stay in the table for historical attribution.</summary>
    public bool IsActive { get; set; } = true;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
