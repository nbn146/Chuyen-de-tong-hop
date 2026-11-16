using ECommerce.Api.Modules.AI.Indexing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Modules.AI.Controllers;

[ApiController]
[Route("api/ai/index")]
[Authorize(Roles = "Admin")]
public sealed class AiIndexController(IAiIndexAdminService indexAdminService) : ControllerBase
{
    [HttpPost("rebuild")]
    public async Task<IActionResult> Rebuild(CancellationToken cancellationToken)
    {
        var count = await indexAdminService.QueueRebuildAsync(cancellationToken);
        return Accepted(new { queuedProducts = count });
    }
}
