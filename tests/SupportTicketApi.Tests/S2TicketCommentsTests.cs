using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace SupportTicketApi.Tests;

/// <summary>
/// Covers the story described in docs/user-stories.md as S2.
/// </summary>
public class S2TicketCommentsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public S2TicketCommentsTests(ApiFactory factory) => _factory = factory;

    /// <summary>The shape the endpoint is expected to return, one entry per comment.</summary>
    private record CommentShape(
        int Id,
        int TicketId,
        string AuthorName,
        string Body,
        bool IsInternal,
        DateTime CreatedUtc);

    private async Task<List<CommentShape>> ReadAsync(string url)
    {
        var client = _factory.CreateClient();
        var comments = await client.GetFromJsonAsync<List<CommentShape>>(url);
        Assert.NotNull(comments);
        return comments!;
    }

    [Fact]
    [Trait("Category", "S2")]
    public async Task Thread_hides_internal_notes_by_default()
    {
        var comments = await ReadAsync("/api/tickets/1/comments");

        Assert.Equal(2, comments.Count);
        Assert.All(comments, c => Assert.False(c.IsInternal));
    }

    [Fact]
    [Trait("Category", "S2")]
    public async Task Thread_is_ordered_oldest_first()
    {
        var comments = await ReadAsync("/api/tickets/1/comments");

        Assert.Equal(
            comments.Select(c => c.CreatedUtc).OrderBy(d => d).ToArray(),
            comments.Select(c => c.CreatedUtc).ToArray());
        Assert.StartsWith("Sign-in fails", comments[0].Body);
    }

    [Fact]
    [Trait("Category", "S2")]
    public async Task Internal_notes_are_returned_when_explicitly_requested()
    {
        var comments = await ReadAsync("/api/tickets/1/comments?includeInternal=true");

        Assert.Equal(3, comments.Count);
        Assert.Single(comments, c => c.IsInternal);
    }

    [Fact]
    [Trait("Category", "S2")]
    public async Task A_ticket_with_no_comments_returns_an_empty_thread()
    {
        var comments = await ReadAsync("/api/tickets/4/comments");

        Assert.Empty(comments);
    }

    [Fact]
    [Trait("Category", "S2")]
    public async Task Reading_the_thread_of_an_unknown_ticket_is_a_404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/tickets/9999/comments");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    [Trait("Category", "S2")]
    public async Task Posting_a_public_comment_appends_it_to_the_thread()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/tickets/2/comments", new
        {
            authorName = "Agent 4",
            body = "Refund has been queued.",
            isInternal = false
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<CommentShape>();
        Assert.NotNull(created);
        Assert.Equal(2, created!.TicketId);
        Assert.False(created.IsInternal);

        var thread = await ReadAsync("/api/tickets/2/comments");
        Assert.Equal(2, thread.Count);
        Assert.Contains(thread, c => c.Body == "Refund has been queued.");
    }

    [Fact]
    [Trait("Category", "S2")]
    public async Task A_posted_internal_note_stays_out_of_the_default_thread()
    {
        var client = _factory.CreateClient();

        await client.PostAsJsonAsync("/api/tickets/3/comments", new
        {
            authorName = "Agent 5",
            body = "Escalating to Tier 2.",
            isInternal = true
        });

        var publicThread = await ReadAsync("/api/tickets/3/comments");
        var fullThread = await ReadAsync("/api/tickets/3/comments?includeInternal=true");

        Assert.Empty(publicThread);
        Assert.Single(fullThread);
    }

    [Fact]
    [Trait("Category", "S2")]
    public async Task Posting_to_an_unknown_ticket_is_a_404()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/tickets/9999/comments", new
        {
            authorName = "Agent 1",
            body = "Nowhere to put this.",
            isInternal = false
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    [Trait("Category", "S2")]
    public async Task Posting_an_empty_body_is_rejected_as_bad_request()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/tickets/1/comments", new
        {
            authorName = "Agent 1",
            body = "",
            isInternal = false
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
