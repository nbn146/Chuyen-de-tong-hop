using ECommerce.Api.Data;
using ECommerce.Api.Features.AI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ECommerce.Api.Features.AI.Indexing;

public sealed class AiIndexJobProcessor(
    AppDbContext dbContext,
    IAiProductIndexer productIndexer,
    IOptions<AiIndexingOptions> options,
    ILogger<AiIndexJobProcessor> logger) : IAiIndexJobProcessor
{
    private readonly AiIndexingOptions _options = options.Value;

    public async Task EnqueueMissingProductsAsync(CancellationToken cancellationToken)
    {
        var productIds = await dbContext.Products
            .Where(product => product.IsActive)
            .Where(product => !dbContext.AiKnowledgeChunks.Any(chunk => chunk.ProductID == product.ProductID))
            .Where(product => !dbContext.AiIndexJobs.Any(job => job.ProductID == product.ProductID))
            .OrderBy(product => product.ProductID)
            .Select(product => product.ProductID)
            .Take(_options.BackfillBatchSize)
            .ToListAsync(cancellationToken);

        if (productIds.Count == 0)
        {
            return;
        }

        dbContext.AiIndexJobs.AddRange(productIds.Select(productId => new AiIndexJob
        {
            ProductID = productId,
            Status = AiIndexStatuses.Pending,
            CreatedAt = DateTime.UtcNow
        }));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ProcessNextAsync(CancellationToken cancellationToken)
    {
        var staleProcessingBefore = DateTime.UtcNow.AddMinutes(-5);
        var job = await dbContext.AiIndexJobs
            .Where(item =>
                (item.Status == AiIndexStatuses.Pending && (item.NextAttemptAt == null || item.NextAttemptAt <= DateTime.UtcNow)) ||
                (item.Status == AiIndexStatuses.Processing && item.StartedAt < staleProcessingBefore))
            .OrderBy(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (job is null)
        {
            return false;
        }

        job.Status = AiIndexStatuses.Processing;
        job.Attempts++;
        job.StartedAt = DateTime.UtcNow;
        job.LastError = null;
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await productIndexer.IndexProductAsync(job.ProductID, cancellationToken);
            job.Status = AiIndexStatuses.Indexed;
            job.CompletedAt = DateTime.UtcNow;
            job.NextAttemptAt = null;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            job.LastError = exception.GetType().Name;
            job.CompletedAt = job.Attempts >= _options.MaxAttempts ? DateTime.UtcNow : null;
            job.Status = job.Attempts >= _options.MaxAttempts ? AiIndexStatuses.Failed : AiIndexStatuses.Pending;
            job.NextAttemptAt = job.Attempts >= _options.MaxAttempts
                ? null
                : DateTime.UtcNow.AddMinutes(Math.Pow(2, Math.Min(job.Attempts, 6)));
            logger.LogWarning(
                "AI indexing job {JobId} failed for product {ProductId} ({ErrorType}).",
                job.JobID,
                job.ProductID,
                exception.GetType().Name);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}

public interface IAiIndexJobProcessor
{
    Task EnqueueMissingProductsAsync(CancellationToken cancellationToken);
    Task<bool> ProcessNextAsync(CancellationToken cancellationToken);
}
