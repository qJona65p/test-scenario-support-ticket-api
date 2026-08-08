using System.Net;
using System.Net.Http.Json;
using SupportTicketApi.Dtos;
using Xunit;

namespace SupportTicketApi.Tests;

public class TicketsRegressionTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public TicketsRegressionTests(ApiFactory factory) => _factory = factory;

    [Fact]
    [Trait("Category", "Regression")]
    public async Task Ticket_detail_returns_the_seeded_ticket()
    {
        var client = _factory.CreateClient();

        var ticket = await client.GetFromJsonAsync<TicketDetailResponse>("/api/tickets/2");

        Assert.NotNull(ticket);
        Assert.Equal("TKT-0002", ticket!.Reference);
        Assert.Equal("High", ticket.Priority);
        Assert.Equal("Assigned", ticket.Status);
        Assert.Equal(2, ticket.AssignedAgentId);
        Assert.Equal("Agent 2", ticket.AssignedAgentName);
        Assert.Null(ticket.ResolvedUtc);
    }

    [Fact]
    [Trait("Category", "Regression")]
    public async Task Ticket_detail_for_an_unknown_id_is_a_problem_details_404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/tickets/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    [Trait("Category", "Regression")]
    public async Task Creating_a_ticket_returns_201_with_a_generated_reference()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/tickets", new
        {
            subject = "Cannot download my receipt",
            description = "The receipt link returns an empty file.",
            priority = "High",
            requesterEmail = "new.requester@example.test"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<TicketDetailResponse>();
        Assert.NotNull(created);
        Assert.Equal("New", created!.Status);
        Assert.Equal("High", created.Priority);
        Assert.Null(created.AssignedAgentId);
        Assert.Equal($"TKT-{created.Id:D4}", created.Reference);
    }

    [Fact]
    [Trait("Category", "Regression")]
    public async Task Creating_a_ticket_without_a_subject_is_rejected_before_the_database()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/tickets", new
        {
            subject = "",
            description = "No subject on this one.",
            priority = "Low",
            requesterEmail = "no.subject@example.test"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    [Trait("Category", "Regression")]
    public async Task The_queue_contains_only_open_tickets()
    {
        var client = _factory.CreateClient();

        var page = await client.GetFromJsonAsync<PagedResult<TicketSummaryResponse>>(
            "/api/tickets?page=1&pageSize=50");

        Assert.NotNull(page);
        Assert.Equal(7, page!.TotalCount);
        Assert.All(page.Items, t => Assert.Contains(t.Status, new[] { "New", "Assigned" }));

        // Sorted before asserting: this proves membership without touching ordering,
        // so it stays green both before and after the B1 fix.
        Assert.Equal(
            new[] { "TKT-0001", "TKT-0002", "TKT-0003", "TKT-0004", "TKT-0005", "TKT-0006", "TKT-0007" },
            page.Items.Select(t => t.Reference).OrderBy(r => r).ToArray());
    }
}
