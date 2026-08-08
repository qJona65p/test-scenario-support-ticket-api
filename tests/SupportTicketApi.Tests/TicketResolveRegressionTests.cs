using System.Net;
using System.Net.Http.Json;
using SupportTicketApi.Dtos;
using Xunit;

namespace SupportTicketApi.Tests;

/// <summary>
/// Its own fixture, and therefore its own database: these tests resolve a ticket
/// that TicketsRegressionTests counts in the open queue.
/// </summary>
public class TicketResolveRegressionTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public TicketResolveRegressionTests(ApiFactory factory) => _factory = factory;

    [Fact]
    [Trait("Category", "Regression")]
    public async Task Resolving_an_open_ticket_stamps_the_resolved_time()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/tickets/7/resolve", content: null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var resolved = await response.Content.ReadFromJsonAsync<TicketDetailResponse>();
        Assert.NotNull(resolved);
        Assert.Equal("Resolved", resolved!.Status);
        Assert.NotNull(resolved.ResolvedUtc);
    }

    [Fact]
    [Trait("Category", "Regression")]
    public async Task Resolving_an_already_resolved_ticket_is_a_409()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/tickets/8/resolve", content: null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    [Trait("Category", "Regression")]
    public async Task Resolving_an_unknown_ticket_is_a_404()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/tickets/9999/resolve", content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
