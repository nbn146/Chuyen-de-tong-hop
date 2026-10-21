using ECommerce.Api.Entities;

namespace ECommerce.Api.Features.AI.Entities;

public sealed class AiKnowledgeChunk
{
    public Guid ChunkID { get; set; }
    public int ProductID { get; set; }
    public int ChunkOrdinal { get; set; }
    public string Content { get; set; } = null!;
    public string ContentHash { get; set; } = null!;
    public string EmbeddingModel { get; set; } = null!;
    public string Status { get; set; } = AiIndexStatuses.Pending;
    public DateTime CreatedAt { get; set; }
    public DateTime? IndexedAt { get; set; }

    public Product Product { get; set; } = null!;
}
