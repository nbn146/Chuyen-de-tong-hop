using ECommerce.Api.Features.AI.Contracts;

namespace ECommerce.Api.Features.AI.Chat;

public interface IChatService
{
    Task<ChatResponseDto> ChatAsync(ChatRequestDto request, int? userId, CancellationToken cancellationToken = default);
}
