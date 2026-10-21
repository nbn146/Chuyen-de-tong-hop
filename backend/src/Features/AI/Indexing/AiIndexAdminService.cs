using ECommerce.Api.Data;
using ECommerce.Api.Features.AI.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Features.AI.Indexing;

public sealed class AiIndexAdminService(AppDbContext dbContext) : IAiIndexAdminService
{
    public async Task<int> QueueRebuildAsync(CancellationToken cancellationToken)
    {
        var productIds = await dbContext.Products
            .Where(product => product.IsActive)
            .Select(product => product.ProductID)
            .ToListAsync(cancellationToken);

        dbContext.AiIndexJobs.AddRange(productIds.Select(productId => new AiIndexJob
        {
            ProductID = productId,
            Status = AiIndexStatuses.Pending,
            CreatedAt = DateTime.UtcNow
        }));
        await dbContext.SaveChangesAsync(cancellationToken);

        return productIds.Count;
    }
}
