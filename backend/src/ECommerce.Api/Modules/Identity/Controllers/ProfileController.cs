using ECommerce.Api.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Modules.Identity;

[ApiController]
[Authorize(Roles = "Customer")]
[Route("api/profile")]
public sealed class ProfileController(IProfileService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ProfileDto>> Get(CancellationToken cancellationToken)
    {
        return Ok(await service.GetAsync(User.GetRequiredUserId(), cancellationToken));
    }

    [HttpPut]
    public async Task<ActionResult<ProfileDto>> Update(
        UpdateProfileDto dto,
        CancellationToken cancellationToken)
    {
        return Ok(await service.UpdateAsync(User.GetRequiredUserId(), dto, cancellationToken));
    }
}
