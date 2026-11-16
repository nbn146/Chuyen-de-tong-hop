using ECommerce.Api.Modules.AI.Entities;

namespace ECommerce.Api.Modules.AI.Chat;

public interface IGeminiAnswerGenerator
{
    Task<string?> GenerateAsync(
        IReadOnlyList<AiChatMessage> history,
        string question,
        string retrievedContext,
        CancellationToken cancellationToken);
}
