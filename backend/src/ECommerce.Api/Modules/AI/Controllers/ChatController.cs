using ECommerce.Api.Shared.Extensions;
using ECommerce.Api.Modules.AI.Chat;
using ECommerce.Api.Modules.AI.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ECommerce.Api.Modules.Identity;

namespace ECommerce.Api.Modules.AI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ChatController(IChatService chatService) : ControllerBase
{
    /// <summary>Send a message to the AI shopping assistant.</summary>
    [HttpPost]
    [EnableRateLimiting("ai-chat")]
    public async Task<ActionResult<ChatResponseDto>> Chat(
        [FromBody] ChatRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Tin nhắn không được để trống."
            });
        }

        int? userId = User.Identity?.IsAuthenticated == true ? User.GetRequiredUserId() : null;
        var response = await chatService.ChatAsync(request, userId, cancellationToken);
        return Ok(response);
    }
}
