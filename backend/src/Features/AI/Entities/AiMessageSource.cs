namespace ECommerce.Api.Features.AI.Entities;

public sealed class AiMessageSource
{
    public long SourceID { get; set; }
    public long MessageID { get; set; }
    public Guid ChunkID { get; set; }
    public int ProductID { get; set; }
    public int Rank { get; set; }
    public double Score { get; set; }
    public string SourceVersion { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public AiChatMessage Message { get; set; } = null!;
    public AiKnowledgeChunk Chunk { get; set; } = null!;
}
