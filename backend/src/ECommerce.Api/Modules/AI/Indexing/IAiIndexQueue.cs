using ECommerce.Api.Entities;

namespace ECommerce.Api.Modules.AI.Indexing;

public interface IAiIndexQueue
{
    void Enqueue(Product product);
    void EnqueueRange(IEnumerable<Product> products);
}
