using System.Net;
using System.Net.Http.Json;
using SupportTicketApi.Dtos;
using Xunit;

namespace SupportTicketApi.Tests;

public class AgentsRegressionTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public AgentsRegressionTests(ApiFactory factory) => _factory = factory;

    [Fact]
    [Trait("Category", "Regression")]
    public async Task Agent_list_returns_every_agent_including_the_inactive_one()
    {
        var client = _factory.CreateClient();

        var agents = await client.GetFromJsonAsync<List<AgentResponse>>("/api/agents");

        Assert.NotNull(agents);
        Assert.Equal(8, agents!.Count);
        Assert.Equal(Enumerable.Range(1, 8), agents.Select(a => a.Id));
        Assert.Single(agents, a => !a.IsActive);
        Assert.Equal(8, agents.Single(a => !a.IsActive).Id);
    }

    [Fact]
    [Trait("Category", "Regression")]
    public async Task Workload_counts_only_open_tickets()
    {
        var client = _factory.CreateClient();

        var first = await client.GetFromJsonAsync<AgentWorkloadResponse>("/api/agents/1/workload");
        var second = await client.GetFromJsonAsync<AgentWorkloadResponse>("/api/agents/2/workload");
        var third = await client.GetFromJsonAsync<AgentWorkloadResponse>("/api/agents/3/workload");

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotNull(third);
        Assert.Equal(2, first!.OpenTicketCount);
        Assert.Equal(1, second!.OpenTicketCount);
        Assert.Equal(0, third!.OpenTicketCount);
    }

    [Fact]
    [Trait("Category", "Regression")]
    public async Task Workload_for_an_unknown_agent_is_a_problem_details_404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/agents/9999/workload");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ProblemShape>();
        Assert.Equal("Not Found", problem?.Title);
        Assert.Equal(404, problem?.Status);
    }

    private record ProblemShape(string? Title, int? Status, string? Detail);
}
