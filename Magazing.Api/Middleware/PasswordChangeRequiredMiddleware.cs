using System.Text.Json;
using Magazing.Application.Services.Security;

namespace Magazing.Api.Middleware;

public class PasswordChangeRequiredMiddleware(RequestDelegate next)
{
    private static readonly string[] AllowedPaths =
    [
        "/api/auth/me",
        "/api/auth/change-password",
        "/api/auth/logout",
        "/api/auth/refresh",
        "/api/auth/login"
    ];

    public async Task InvokeAsync(HttpContext context)
    {
        var mustChange = context.User.Identity?.IsAuthenticated == true
            && context.User.HasClaim(TemporaryPassword.ClaimType, "true");

        var path = context.Request.Path.Value ?? string.Empty;
        if (mustChange && path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase)
            && !AllowedPaths.Any(allowed => path.Equals(allowed, StringComparison.OrdinalIgnoreCase)))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "Debes cambiar tu contraseña temporal antes de continuar."
            }));
            return;
        }

        await next(context);
    }
}
