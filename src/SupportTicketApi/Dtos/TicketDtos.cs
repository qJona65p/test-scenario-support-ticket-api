using System.ComponentModel.DataAnnotations;
using SupportTicketApi.Models;

namespace SupportTicketApi.Dtos;

public record TicketSummaryResponse(
    int Id,
    string Reference,
    string Subject,
    string Priority,
    string Status,
    int? AssignedAgentId,
    DateTime CreatedUtc);

public record TicketDetailResponse(
    int Id,
    string Reference,
    string Subject,
    string Description,
    string Priority,
    string Status,
    string RequesterEmail,
    int? AssignedAgentId,
    string? AssignedAgentName,
    DateTime CreatedUtc,
    DateTime? ResolvedUtc);

public class CreateTicketRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public TicketPriority Priority { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string RequesterEmail { get; set; } = string.Empty;
}

/// <summary>Query string parameters for the open ticket queue.</summary>
public class TicketQueueRequest
{
    /// <summary>1-based page number.</summary>
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 200)]
    public int PageSize { get; set; } = 20;
}

public class AssignTicketRequest{
    [Range(1, int.MaxValue)]
    public int AgentId { get; set; }
}

public class TicketCommentRequest{
    [StringLength(200, MinimumLength = 1)] public string AuthorName { get; set; }
    [StringLength(200, MinimumLength = 1)] public string Body { get; set; }
    public bool IsInternal { get; set; }
}

public record TicketCommentResponse(
    int Id,
    int TicketId,
    string AuthorName,
    string Body,
    bool IsInternal,
    DateTime CreatedUtc
);