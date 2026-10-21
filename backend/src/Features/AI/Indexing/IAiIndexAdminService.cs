namespace ECommerce.Api.Features.AI.Indexing;

public interface IAiIndexAdminService
{
    Task<int> QueueRebuildAsync(CancellationToken cancellationToken);
}
