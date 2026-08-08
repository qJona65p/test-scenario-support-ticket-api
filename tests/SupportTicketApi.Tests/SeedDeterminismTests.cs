using Microsoft.EntityFrameworkCore;
using SupportTicketApi.Data;
using SupportTicketApi.Models;
using Xunit;

namespace SupportTicketApi.Tests;

/// <summary>
/// The seed is the fixture every other test reasons about. If it drifts, every
/// expectation in the suite becomes unreliable, so it is pinned here.
/// </summary>
public class SeedDeterminismTests
{
    private static AppDbContext NewSeededContext(out string path)
    {
        path = Path.Combine(Path.GetTempPath(), $"seed-tests-{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={path}")
            .Options;
        var db = new AppDbContext(options);
        db.Database.Migrate();
        DbSeeder.Seed(db);
        return db;
    }

    private static void Cleanup(params string[] paths)
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        foreach (var path in paths)
        {
            try
            {
                File.Delete(path);
            }
            catch (IOException)
            {
                // Best effort - a leaked temp file must not fail the suite.
            }
        }
    }

    [Fact]
    [Trait("Category", "Regression")]
    public void Seed_produces_the_documented_row_counts()
    {
        var db = NewSeededContext(out var path);

        Assert.Equal(8, db.Agents.Count());
        Assert.Equal(45, db.Tickets.Count());
        Assert.Equal(6, db.Comments.Count());
        Assert.Equal(7, db.Tickets.Count(t =>
            t.Status == TicketStatus.New || t.Status == TicketStatus.Assigned));

        db.Dispose();
        Cleanup(path);
    }

    [Fact]
    [Trait("Category", "Regression")]
    public void Seed_is_identical_across_two_independent_databases()
    {
        var first = NewSeededContext(out var firstPath);
        var second = NewSeededContext(out var secondPath);

        static string Fingerprint(AppDbContext db) => string.Join("|", db.Tickets
            .OrderBy(t => t.Id)
            .Select(t => $"{t.Reference}:{t.Priority}:{t.Status}:{t.CreatedUtc:O}:{t.AssignedAgentId}")
            .ToList());

        Assert.Equal(Fingerprint(first), Fingerprint(second));

        first.Dispose();
        second.Dispose();
        Cleanup(firstPath, secondPath);
    }

    [Fact]
    [Trait("Category", "Regression")]
    public void The_open_queue_fixture_matches_the_documented_table()
    {
        var db = NewSeededContext(out var path);

        var open = db.Tickets
            .Where(t => t.Status == TicketStatus.New || t.Status == TicketStatus.Assigned)
            .OrderBy(t => t.Id)
            .Select(t => new { t.Reference, t.Priority, t.CreatedUtc })
            .ToList();

        Assert.Equal(
            new[] { "TKT-0001", "TKT-0002", "TKT-0003", "TKT-0004", "TKT-0005", "TKT-0006", "TKT-0007" },
            open.Select(t => t.Reference).ToArray());
        Assert.Equal(
            new[]
            {
                TicketPriority.Urgent, TicketPriority.High, TicketPriority.Normal,
                TicketPriority.Low, TicketPriority.Normal, TicketPriority.Normal,
                TicketPriority.Normal
            },
            open.Select(t => t.Priority).ToArray());
        Assert.Equal(
            Enumerable.Range(1, 7).Select(n => DbSeeder.SeedBaseUtc.AddMinutes(n * 10)).ToArray(),
            open.Select(t => t.CreatedUtc).ToArray());

        db.Dispose();
        Cleanup(path);
    }
}
