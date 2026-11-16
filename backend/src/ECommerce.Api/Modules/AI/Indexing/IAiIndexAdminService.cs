namespace ECommerce.Api.Modules.AI.Indexing;

public interface IAiIndexAdminService
{
    Task<int> QueueRebuildAsync(CancellationToken cancellationToken);
}
