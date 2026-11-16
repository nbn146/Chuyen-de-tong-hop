using ECommerce.Api.Features.AI.Entities;

namespace ECommerce.Api.Features.AI.Chat;

public interface IGeminiAnswerGenerator
{
    Task<string?> GenerateAsync(
        IReadOnlyList<AiChatMessage> history,
        string question,
        string retrievedContext,
        CancellationToken cancellationToken);
}
