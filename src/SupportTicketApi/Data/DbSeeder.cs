using SupportTicketApi.Models;

namespace SupportTicketApi.Data;

/// <summary>
/// Deterministic seed data. Every value derives arithmetically from a row index
/// or from <see cref="SeedBaseUtc"/> so the database is byte-identical on every run.
/// Never introduce DateTime.Now, Guid.NewGuid, or unseeded Random here.
/// </summary>
public static class DbSeeder
{
    public static readonly DateTime SeedBaseUtc = new(2026, 2, 2, 9, 0, 0, DateTimeKind.Utc);

    private const int AgentCount = 8;
    private const int TicketCount = 45;
    private const int OpenTicketCount = 7;

    private static readonly string[] Teams = ["Tier 1", "Tier 2", "Billing"];

    private static readonly string[] Subjects =
    [
        "Cannot sign in",
        "Invoice is wrong",
        "Export never finishes",
        "Password reset email missing",
        "Report shows stale data",
        "Attachment upload fails",
        "Duplicate charge",
        "Search returns nothing"
    ];

    public static void Seed(AppDbContext db)
    {
        if (db.Agents.Any())
        {
            return;
        }

        db.Agents.AddRange(BuildAgents());
        db.SaveChanges();

        db.Tickets.AddRange(BuildTickets());
        db.SaveChanges();

        db.Comments.AddRange(BuildComments());
        db.SaveChanges();
    }

    private static List<Agent> BuildAgents()
    {
        var agents = new List<Agent>(AgentCount);
        for (var id = 1; id <= AgentCount; id++)
        {
            agents.Add(new Agent
            {
                Id = id,
                Name = $"Agent {id}",
                Email = $"agent{id}@example.test",
                Team = Teams[(id - 1) % Teams.Length],
                // The last agent has left the team; their tickets stay attributed.
                IsActive = id != AgentCount
            });
        }

        return agents;
    }

    /// <summary>
    /// Tickets 1-7 are the open queue and are fixed by hand - one per priority plus
    /// three extra Normal tickets - so queue contents are exactly reproducible.
    /// Tickets 8-45 are all resolved or closed and never appear in the queue.
    /// </summary>
    private static List<Ticket> BuildTickets()
    {
        var tickets = new List<Ticket>(TicketCount)
        {
            Open(1, TicketPriority.Urgent, TicketStatus.New,      minutes: 10, agentId: null),
            Open(2, TicketPriority.High,   TicketStatus.Assigned, minutes: 20, agentId: 2),
            Open(3, TicketPriority.Normal, TicketStatus.New,      minutes: 30, agentId: null),
            Open(4, TicketPriority.Low,    TicketStatus.New,      minutes: 40, agentId: null),
            Open(5, TicketPriority.Normal, TicketStatus.Assigned, minutes: 50, agentId: 1),
            Open(6, TicketPriority.Normal, TicketStatus.Assigned, minutes: 60, agentId: 1),
            Open(7, TicketPriority.Normal, TicketStatus.New,      minutes: 70, agentId: null)
        };

        for (var id = OpenTicketCount + 1; id <= TicketCount; id++)
        {
            var createdUtc = SeedBaseUtc.AddHours(-(TicketCount - id) - 1);
            tickets.Add(new Ticket
            {
                Id = id,
                Reference = $"TKT-{id:D4}",
                Subject = Subjects[(id - 1) % Subjects.Length],
                Description = $"Reported through the web form. Reference {id}.",
                Priority = (TicketPriority)((id - 1) % 4),
                Status = id % 2 == 0 ? TicketStatus.Resolved : TicketStatus.Closed,
                RequesterEmail = $"user{id}@example.test",
                AssignedAgentId = 1 + (id - 1) % AgentCount,
                CreatedUtc = createdUtc,
                ResolvedUtc = createdUtc.AddHours(4)
            });
        }

        return tickets;
    }

    private static Ticket Open(int id, TicketPriority priority, TicketStatus status, int minutes, int? agentId) =>
        new()
        {
            Id = id,
            Reference = $"TKT-{id:D4}",
            Subject = Subjects[(id - 1) % Subjects.Length],
            Description = $"Reported through the web form. Reference {id}.",
            Priority = priority,
            Status = status,
            RequesterEmail = $"user{id}@example.test",
            AssignedAgentId = agentId,
            CreatedUtc = SeedBaseUtc.AddMinutes(minutes),
            ResolvedUtc = null
        };

    private static List<Comment> BuildComments() =>
    [
        new()
        {
            TicketId = 1,
            AuthorName = "Dana Whitfield",
            Body = "Sign-in fails right after the password prompt.",
            IsInternal = false,
            CreatedUtc = SeedBaseUtc.AddMinutes(15)
        },
        new()
        {
            TicketId = 1,
            AuthorName = "Agent 3",
            Body = "Clock skew on the edge nodes. Do not share this with the requester yet.",
            IsInternal = true,
            CreatedUtc = SeedBaseUtc.AddMinutes(25)
        },
        new()
        {
            TicketId = 1,
            AuthorName = "Agent 3",
            Body = "We have reproduced this and are working on it.",
            IsInternal = false,
            CreatedUtc = SeedBaseUtc.AddMinutes(35)
        },
        new()
        {
            TicketId = 2,
            AuthorName = "Ravi Lindqvist",
            Body = "The invoice total does not match my order.",
            IsInternal = false,
            CreatedUtc = SeedBaseUtc.AddMinutes(22)
        },
        new()
        {
            TicketId = 8,
            AuthorName = "Agent 1",
            Body = "Closed after the requester confirmed the fix.",
            IsInternal = false,
            CreatedUtc = SeedBaseUtc.AddHours(-36)
        },
        new()
        {
            TicketId = 8,
            AuthorName = "Agent 1",
            Body = "Root cause was a stale cache entry.",
            IsInternal = true,
            CreatedUtc = SeedBaseUtc.AddHours(-35)
        }
    ];
}
