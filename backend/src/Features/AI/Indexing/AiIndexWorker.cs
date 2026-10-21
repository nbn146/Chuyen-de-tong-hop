namespace ECommerce.Api.Features.AI.Indexing;

public sealed class AiIndexWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<AiIndexWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var lastBackfillAt = DateTime.UtcNow;
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<IAiIndexJobProcessor>()
                .EnqueueMissingProductsAsync(stoppingToken);
            lastBackfillAt = DateTime.UtcNow;
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception exception)
        {
            logger.LogWarning("AI product index backfill could not be initialized ({ErrorType}).", exception.GetType().Name);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var processed = false;
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                processed = await scope.ServiceProvider.GetRequiredService<IAiIndexJobProcessor>()
                    .ProcessNextAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogWarning("AI index worker encountered an error ({ErrorType}).", exception.GetType().Name);
            }

            if (!processed)
            {
                if (DateTime.UtcNow - lastBackfillAt >= TimeSpan.FromMinutes(1))
                {
                    try
                    {
                        await using var scope = scopeFactory.CreateAsyncScope();
                        await scope.ServiceProvider.GetRequiredService<IAiIndexJobProcessor>()
                            .EnqueueMissingProductsAsync(stoppingToken);
                        lastBackfillAt = DateTime.UtcNow;
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception exception)
                    {
                        logger.LogWarning("AI product index backfill could not be refreshed ({ErrorType}).", exception.GetType().Name);
                        lastBackfillAt = DateTime.UtcNow;
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
