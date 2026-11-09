using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Magazing.Api.Authorization;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException("El token no contiene un usuario valido.");
        }

        return userId;
    }
}
