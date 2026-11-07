namespace ECommerce.Api.Features.AI.Entities;

public sealed class AiMessageSource
{
    public long SourceID { get; set; }
    public long MessageID { get; set; }

    /// <summary>How the product was found; one of <see cref="AiSourceTypes"/>.</summary>
    public string SourceType { get; set; } = AiSourceTypes.Vector;

    /// <summary>Set only for <see cref="AiSourceTypes.Vector"/> sources.</summary>
    public Guid? ChunkID { get; set; }

    public int ProductID { get; set; }
    public int Rank { get; set; }
    public double Score { get; set; }

    /// <summary>Content hash of the chunk used; set only for <see cref="AiSourceTypes.Vector"/> sources.</summary>
    public string? SourceVersion { get; set; }

    public DateTime CreatedAt { get; set; }

    public AiChatMessage Message { get; set; } = null!;
    public AiKnowledgeChunk? Chunk { get; set; }
}

public static class AiSourceTypes
{
    /// <summary>Found through vector retrieval of an indexed knowledge chunk.</summary>
    public const string Vector = "vector";

    /// <summary>Found through SQL keyword/filter search without a chunk.</summary>
    public const string Lexical = "lexical";

    /// <summary>Loaded directly from the catalog, for example for comparisons.</summary>
    public const string Catalog = "catalog";
}
