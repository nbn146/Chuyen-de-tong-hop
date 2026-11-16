using ECommerce.Api.Features.AI.Chat;
using ECommerce.Api.Features.AI.Contracts;
using ECommerce.Api.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.Api.Features.AI.Controllers;

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

        int? userId = User.Identity?.IsAuthenticated == true ? User.GetUserId() : null;
        var response = await chatService.ChatAsync(request, userId, cancellationToken);
        return Ok(response);
    }
}
