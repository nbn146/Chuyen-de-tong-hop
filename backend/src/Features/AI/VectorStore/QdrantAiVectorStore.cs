using Grpc.Core;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace ECommerce.Api.Features.AI.VectorStore;

public sealed class QdrantAiVectorStore(
    QdrantClient client,
    IOptions<AiQdrantOptions> options) : IAiVectorStore
{
    private readonly string _collectionName = options.Value.Collection;
    private readonly SemaphoreSlim _collectionLock = new(1, 1);

    public async Task UpsertAsync(
        Guid chunkId,
        int productId,
        string contentHash,
        float[] vector,
        CancellationToken cancellationToken)
    {
        await EnsureCollectionAsync(vector.Length, cancellationToken);

        var point = new PointStruct
        {
            Id = chunkId,
            Vectors = vector,
            Payload =
            {
                ["product_id"] = productId,
                ["chunk_id"] = chunkId.ToString(),
                ["content_hash"] = contentHash
            }
        };

        await client.UpsertAsync(_collectionName, [point], cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<AiVectorMatch>> SearchAsync(
        float[] vector,
        int limit,
        CancellationToken cancellationToken)
    {
        if (!await client.CollectionExistsAsync(_collectionName, cancellationToken))
        {
            return [];
        }

#pragma warning disable CS0618
        var points = await client.SearchAsync(
            _collectionName,
            vector,
            limit: (ulong)limit,
            cancellationToken: cancellationToken);
#pragma warning restore CS0618

        var matches = new List<AiVectorMatch>(points.Count);
        foreach (var point in points)
        {
            if (!point.Payload.TryGetValue("product_id", out var productId) || productId.IntegerValue <= 0 ||
                !point.Payload.TryGetValue("chunk_id", out var chunkId) ||
                !point.Payload.TryGetValue("content_hash", out var contentHash) ||
                !Guid.TryParse(chunkId.StringValue, out var parsedChunkId))
            {
                continue;
            }

            matches.Add(new AiVectorMatch(
                parsedChunkId,
                checked((int)productId.IntegerValue),
                contentHash.StringValue,
                point.Score));
        }

        return matches;
    }

    public async Task DeleteAsync(Guid chunkId, CancellationToken cancellationToken)
    {
        if (await client.CollectionExistsAsync(_collectionName, cancellationToken))
        {
            await client.DeleteAsync(_collectionName, chunkId, cancellationToken: cancellationToken);
        }
    }

    private async Task EnsureCollectionAsync(int vectorSize, CancellationToken cancellationToken)
    {
        if (await client.CollectionExistsAsync(_collectionName, cancellationToken))
        {
            return;
        }

        await _collectionLock.WaitAsync(cancellationToken);
        try
        {
            if (!await client.CollectionExistsAsync(_collectionName, cancellationToken))
            {
                await client.CreateCollectionAsync(
                    _collectionName,
                    new VectorParams { Size = (ulong)vectorSize, Distance = Distance.Cosine },
                    cancellationToken: cancellationToken);
            }
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.AlreadyExists)
        {
            // Another API instance created the same collection at the same time.
        }
        finally
        {
            _collectionLock.Release();
        }
    }
}

public sealed class AiQdrantOptions
{
    public string Endpoint { get; set; } = "http://localhost:6334";
    public string Collection { get; set; } = "ecommerce_products_v1";
    public string? ApiKey { get; set; }
}
