using System.ComponentModel.DataAnnotations;

namespace ECommerce.Api.Features.AI.Contracts;

public record ChatRequestDto(
    [property: Required, StringLength(2000, MinimumLength = 1)] string Message,
    [property: MaxLength(20)] List<ChatMessageDto>? History,
    Guid? SessionId = null,
    [property: StringLength(128)] string? SessionToken = null);

public record ChatMessageDto(
    [property: Required, StringLength(16)] string Role,
    [property: Required, StringLength(2000)] string Content);

public record ChatResponseDto(
    string Reply,
    List<RecommendedProductDto>? Products,
    Guid SessionId,
    string? SessionToken,
    List<ChatSourceDto> Sources);

public record ChatSourceDto(int ProductId, string ProductName, double Score);

public record RecommendedProductDto(
    int ProductId,
    string ProductName,
    decimal Price,
    string? ImageUrl,
    string? Reason
);
