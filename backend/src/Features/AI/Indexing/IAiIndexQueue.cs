using ECommerce.Api.Entities;

namespace ECommerce.Api.Features.AI.Indexing;

public interface IAiIndexQueue
{
    void Enqueue(Product product);
    void EnqueueRange(IEnumerable<Product> products);
}
