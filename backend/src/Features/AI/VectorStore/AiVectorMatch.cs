namespace ECommerce.Api.Features.AI.VectorStore;

public sealed record AiVectorMatch(Guid ChunkID, int ProductID, string ContentHash, double Score);
