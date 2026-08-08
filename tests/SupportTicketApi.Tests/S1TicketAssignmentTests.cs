using System.Net;
using System.Net.Http.Json;
using SupportTicketApi.Dtos;
using Xunit;

namespace SupportTicketApi.Tests;

/// <summary>
/// Covers the story described in docs/user-stories.md as S1.
/// </summary>
public class S1TicketAssignmentTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public S1TicketAssignmentTests(ApiFactory factory) => _factory = factory;

    private static HttpContent Body(int agentId) => JsonContent.Create(new { agentId });

    [Fact]
    [Trait("Category", "S1")]
    public async Task Assigning_an_unassigned_ticket_moves_it_to_assigned()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/tickets/3/assign", Body(3));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var ticket = await response.Content.ReadFromJsonAsync<TicketDetailResponse>();
        Assert.NotNull(ticket);
        Assert.Equal("Assigned", ticket!.Status);
        Assert.Equal(3, ticket.AssignedAgentId);
        Assert.Equal("Agent 3", ticket.AssignedAgentName);
    }

    [Fact]
    [Trait("Category", "S1")]
    public async Task Assignment_is_visible_on_a_subsequent_read()
    {
        var client = _factory.CreateClient();

        await client.PostAsync("/api/tickets/7/assign", Body(5));
        var ticket = await client.GetFromJsonAsync<TicketDetailResponse>("/api/tickets/7");

        Assert.NotNull(ticket);
        Assert.Equal("Assigned", ticket!.Status);
        Assert.Equal(5, ticket.AssignedAgentId);
    }

    [Fact]
    [Trait("Category", "S1")]
    public async Task Assignment_increases_the_agent_open_workload()
    {
        var client = _factory.CreateClient();

        var before = await client.GetFromJsonAsync<AgentWorkloadResponse>("/api/agents/6/workload");
        await client.PostAsync("/api/tickets/2/assign", Body(6));
        var after = await client.GetFromJsonAsync<AgentWorkloadResponse>("/api/agents/6/workload");

        Assert.NotNull(before);
        Assert.NotNull(after);
        Assert.Equal(before!.OpenTicketCount + 1, after!.OpenTicketCount);
    }

    [Fact]
    [Trait("Category", "S1")]
    public async Task Assigning_an_unknown_ticket_is_a_404()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/tickets/9999/assign", Body(1));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    [Trait("Category", "S1")]
    public async Task Assigning_to_an_unknown_agent_is_a_404()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/tickets/1/assign", Body(9999));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    [Trait("Category", "S1")]
    public async Task Assigning_to_an_inactive_agent_is_a_409()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/tickets/1/assign", Body(8));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    [Trait("Category", "S1")]
    public async Task Assigning_a_resolved_ticket_is_a_409()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/tickets/8/assign", Body(1));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    [Trait("Category", "S1")]
    public async Task Assigning_a_closed_ticket_is_a_409()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/tickets/9/assign", Body(1));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    [Trait("Category", "S1")]
    public async Task A_missing_agent_id_is_rejected_as_bad_request()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/tickets/1/assign", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
