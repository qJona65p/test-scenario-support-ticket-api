using Microsoft.EntityFrameworkCore;
using SupportTicketApi.Models;

namespace SupportTicketApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agent>(entity =>
        {
            entity.Property(a => a.Name).HasMaxLength(200).IsRequired();
            entity.Property(a => a.Email).HasMaxLength(320).IsRequired();
            entity.Property(a => a.Team).HasMaxLength(64).IsRequired();
            entity.HasIndex(a => a.Email).IsUnique();
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.Property(t => t.Reference).HasMaxLength(16).IsRequired();
            entity.Property(t => t.Subject).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Description).HasMaxLength(4000).IsRequired();
            entity.Property(t => t.RequesterEmail).HasMaxLength(320).IsRequired();

            // Stored as text so the database reads cleanly in a SQLite viewer.
            entity.Property(t => t.Priority).HasConversion<string>().HasMaxLength(16);
            entity.Property(t => t.Status).HasConversion<string>().HasMaxLength(16);

            entity.HasIndex(t => t.Reference).IsUnique();
            entity.HasIndex(t => t.Status);
            entity.HasOne(t => t.AssignedAgent).WithMany(a => a.Tickets)
                  .HasForeignKey(t => t.AssignedAgentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.Property(c => c.AuthorName).HasMaxLength(200).IsRequired();
            entity.Property(c => c.Body).HasMaxLength(4000).IsRequired();
            entity.HasOne(c => c.Ticket).WithMany(t => t.Comments)
                  .HasForeignKey(c => c.TicketId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(c => new { c.TicketId, c.CreatedUtc });
        });
    }
}
