using Magazing.Api.Authorization;
using Magazing.Api.Configuration;
using Magazing.Application.DTOs.Auth;
using Magazing.Application.DTOs.Settings;
using Magazing.Application.Services;
using Magazing.Application.UseCases.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, ISender sender) : ControllerBase
{
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        return Ok(await sender.Send(new GetUserByIdQuery(User.GetUserId())));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(SecurityExtensions.LoginRateLimitPolicy)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        return Ok(await authService.LoginAsync(request.Username, request.Password, GetIpAddress(), cancellationToken));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        return Ok(await authService.RefreshAsync(request.RefreshToken, GetIpAddress(), cancellationToken));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        await authService.ChangePasswordAsync(User.GetUserId(), request.CurrentPassword, request.NewPassword, cancellationToken);
        return NoContent();
    }

    private string? GetIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}
