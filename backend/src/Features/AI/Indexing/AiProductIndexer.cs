using ECommerce.Api.Data;
using ECommerce.Api.Features.AI.Entities;
using ECommerce.Api.Features.AI.VectorStore;
using Microsoft.Extensions.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ECommerce.Api.Features.AI.Indexing;

public sealed class AiProductIndexer(
    AppDbContext dbContext,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IAiVectorStore vectorStore,
    IOptions<AiIndexingOptions> options) : IAiProductIndexer
{
    private readonly AiIndexingOptions _options = options.Value;

    public async Task IndexProductAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .Include(item => item.Category)
            .Include(item => item.Brand)
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.ProductID == productId, cancellationToken);

        var chunk = await dbContext.AiKnowledgeChunks
            .FirstOrDefaultAsync(item => item.ProductID == productId && item.ChunkOrdinal == 0, cancellationToken);

        if (product is null || !product.IsActive)
        {
            if (chunk is not null)
            {
                await vectorStore.DeleteAsync(chunk.ChunkID, cancellationToken);
                chunk.Status = AiIndexStatuses.Inactive;
                chunk.IndexedAt = null;
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return;
        }

        var document = AiProductDocumentBuilder.Build(product);
        if (chunk is not null && chunk.Status == AiIndexStatuses.Indexed && chunk.ContentHash == document.Hash)
        {
            return;
        }

        chunk ??= new AiKnowledgeChunk
        {
            ChunkID = Guid.NewGuid(),
            ProductID = productId,
            ChunkOrdinal = 0,
            CreatedAt = DateTime.UtcNow
        };

        chunk.Content = document.Content;
        chunk.ContentHash = document.Hash;
        chunk.EmbeddingModel = _options.EmbeddingModel;
        chunk.Status = AiIndexStatuses.Pending;
        chunk.IndexedAt = null;
        if (dbContext.Entry(chunk).State == EntityState.Detached)
        {
            dbContext.AiKnowledgeChunks.Add(chunk);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var embedding = await embeddingGenerator.GenerateAsync(document.Content, cancellationToken: cancellationToken);
        await vectorStore.UpsertAsync(chunk.ChunkID, productId, document.Hash, embedding.Vector.ToArray(), cancellationToken);

        chunk.Status = AiIndexStatuses.Indexed;
        chunk.IndexedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed class AiIndexingOptions
{
    public string EmbeddingModel { get; set; } = "bge-m3";
    public int BackfillBatchSize { get; set; } = 200;
    public int MaxAttempts { get; set; } = 5;
}
