using ECommerce.Api.Data;
using ECommerce.Api.Entities;
using ECommerce.Api.Features.AI.Entities;

namespace ECommerce.Api.Features.AI.Indexing;

public sealed class AiIndexQueue(AppDbContext dbContext) : IAiIndexQueue
{
    public void Enqueue(Product product)
    {
        dbContext.AiIndexJobs.Add(new AiIndexJob
        {
            ProductID = product.ProductID,
            Product = product,
            Status = AiIndexStatuses.Pending,
            CreatedAt = DateTime.UtcNow
        });
    }

    public void EnqueueRange(IEnumerable<Product> products)
    {
        foreach (var product in products)
        {
            Enqueue(product);
        }
    }
}
