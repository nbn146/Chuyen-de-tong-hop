using ECommerce.Api.Entities;

namespace ECommerce.Api.Features.AI.Entities;

public sealed class AiIndexJob
{
    public long JobID { get; set; }
    public int ProductID { get; set; }
    public string Status { get; set; } = AiIndexStatuses.Pending;
    public int Attempts { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? NextAttemptAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Product Product { get; set; } = null!;
}
