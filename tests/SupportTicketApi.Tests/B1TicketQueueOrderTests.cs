using System.Net.Http.Json;
using SupportTicketApi.Dtos;
using Xunit;

namespace SupportTicketApi.Tests;

/// <summary>
/// Covers the defect reported in docs/bug-reports.md as B1.
/// </summary>
public class B1TicketQueueOrderTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public B1TicketQueueOrderTests(ApiFactory factory) => _factory = factory;

    private async Task<string[]> QueueReferencesAsync()
    {
        var client = _factory.CreateClient();
        var page = await client.GetFromJsonAsync<PagedResult<TicketSummaryResponse>>(
            "/api/tickets?page=1&pageSize=50");
        Assert.NotNull(page);
        return page!.Items.Select(t => t.Reference).ToArray();
    }

    [Fact]
    [Trait("Category", "B1")]
    public async Task Queue_is_ordered_most_severe_first_then_oldest_first()
    {
        Assert.Equal(
            new[]
            {
                "TKT-0001", // Urgent
                "TKT-0002", // High
                "TKT-0003", // Normal, oldest
                "TKT-0005", // Normal
                "TKT-0006", // Normal
                "TKT-0007", // Normal, newest
                "TKT-0004"  // Low
            },
            await QueueReferencesAsync());
    }

    [Fact]
    [Trait("Category", "B1")]
    public async Task High_priority_tickets_rank_above_normal_and_low()
    {
        var references = await QueueReferencesAsync();

        var high = Array.IndexOf(references, "TKT-0002");
        var normal = Array.IndexOf(references, "TKT-0003");
        var low = Array.IndexOf(references, "TKT-0004");

        Assert.True(high < normal, $"High ranked at {high}, Normal at {normal}.");
        Assert.True(high < low, $"High ranked at {high}, Low at {low}.");
    }

    [Fact]
    [Trait("Category", "B1")]
    public async Task Queue_ranks_priorities_most_severe_first()
    {
        var client = _factory.CreateClient();
        var page = await client.GetFromJsonAsync<PagedResult<TicketSummaryResponse>>(
            "/api/tickets?page=1&pageSize=50");
        Assert.NotNull(page);

        Assert.Equal(
            new[] { "Urgent", "High", "Normal", "Normal", "Normal", "Normal", "Low" },
            page!.Items.Select(t => t.Priority).ToArray());
    }

    [Fact]
    [Trait("Category", "B1")]
    public async Task Low_priority_sinks_to_the_bottom()
    {
        var references = await QueueReferencesAsync();

        Assert.Equal("TKT-0004", references[^1]);
    }

    [Fact]
    [Trait("Category", "B1")]
    public async Task Tickets_of_equal_priority_are_oldest_first()
    {
        var references = await QueueReferencesAsync();

        var normals = references
            .Where(r => r is "TKT-0003" or "TKT-0005" or "TKT-0006" or "TKT-0007")
            .ToArray();

        Assert.Equal(new[] { "TKT-0003", "TKT-0005", "TKT-0006", "TKT-0007" }, normals);
    }

    [Fact]
    [Trait("Category", "B1")]
    public async Task Paging_the_queue_preserves_the_ranking()
    {
        var client = _factory.CreateClient();
        var seen = new List<string>();

        for (var page = 1; page <= 4; page++)
        {
            var result = await client.GetFromJsonAsync<PagedResult<TicketSummaryResponse>>(
                $"/api/tickets?page={page}&pageSize=2");
            Assert.NotNull(result);
            seen.AddRange(result!.Items.Select(t => t.Reference));
        }

        Assert.Equal(
            new[] { "TKT-0001", "TKT-0002", "TKT-0003", "TKT-0005", "TKT-0006", "TKT-0007", "TKT-0004" },
            seen.ToArray());
    }
}
