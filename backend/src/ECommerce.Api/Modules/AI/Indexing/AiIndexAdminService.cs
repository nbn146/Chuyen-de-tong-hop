using ECommerce.Api.Modules.Catalog.Contracts;
using ECommerce.Api.Data;
using ECommerce.Api.Modules.AI.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Modules.AI.Indexing;

public sealed class AiIndexAdminService(AppDbContext dbContext, IProductCatalog catalog) : IAiIndexAdminService
{
    public async Task<int> QueueRebuildAsync(CancellationToken cancellationToken)
    {
        var productIds = await catalog.GetActiveProductIdsAsync(cancellationToken);

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
