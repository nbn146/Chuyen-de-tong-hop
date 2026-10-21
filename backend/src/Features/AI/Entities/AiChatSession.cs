namespace ECommerce.Api.Features.AI.Entities;

public sealed class AiChatSession
{
    public Guid SessionID { get; set; }
    public int? UserID { get; set; }
    public string? GuestTokenHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    public ICollection<AiChatMessage> Messages { get; set; } = new List<AiChatMessage>();
}
