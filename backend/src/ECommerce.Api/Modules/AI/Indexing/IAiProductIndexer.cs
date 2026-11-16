namespace ECommerce.Api.Modules.AI.Indexing;

public interface IAiProductIndexer
{
    Task IndexProductAsync(int productId, CancellationToken cancellationToken);
}
