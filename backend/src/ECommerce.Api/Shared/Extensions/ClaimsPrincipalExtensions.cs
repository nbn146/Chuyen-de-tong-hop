using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ECommerce.Api.Shared.Exceptions;

namespace ECommerce.Api.Shared.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetRequiredUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(value, out var userId) || userId <= 0)
        {
            throw new InvalidUserIdentityException();
        }

        return userId;
    }
}
