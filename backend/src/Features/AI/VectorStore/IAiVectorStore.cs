namespace ECommerce.Api.Features.AI.VectorStore;

public interface IAiVectorStore
{
    Task UpsertAsync(Guid chunkId, int productId, string contentHash, float[] vector, CancellationToken cancellationToken);
    Task<IReadOnlyList<AiVectorMatch>> SearchAsync(float[] vector, int limit, CancellationToken cancellationToken);
    Task DeleteAsync(Guid chunkId, CancellationToken cancellationToken);
}
