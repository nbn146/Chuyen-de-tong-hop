using ECommerce.Api.Modules.AI.Contracts;

namespace ECommerce.Api.Modules.AI.Chat;

public interface IChatService
{
    Task<ChatResponseDto> ChatAsync(ChatRequestDto request, int? userId, CancellationToken cancellationToken = default);
}
