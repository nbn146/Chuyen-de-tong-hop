namespace ECommerce.Api.Features.AI.Indexing;

public interface IAiProductIndexer
{
    Task IndexProductAsync(int productId, CancellationToken cancellationToken);
}
