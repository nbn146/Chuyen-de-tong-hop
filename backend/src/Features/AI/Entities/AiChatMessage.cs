namespace ECommerce.Api.Features.AI.Entities;

public sealed class AiChatMessage
{
    public long MessageID { get; set; }
    public Guid SessionID { get; set; }
    public string Role { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? AnswerType { get; set; }
    public string? ModelName { get; set; }
    public DateTime CreatedAt { get; set; }

    public AiChatSession Session { get; set; } = null!;
    public ICollection<AiMessageSource> Sources { get; set; } = new List<AiMessageSource>();
}
