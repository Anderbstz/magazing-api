using Magazing.Api.Configuration;
using Magazing.Application.DTOs.Assistant;
using Magazing.Application.Services.Security;
using Magazing.Application.UseCases.Assistant.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssistantController(ISender sender) : ControllerBase
{
    [HttpPost("chat")]
    [EnableRateLimiting(SecurityExtensions.AssistantRateLimitPolicy)]
    public async Task<IActionResult> Chat([FromBody] AssistantChatRequest request, CancellationToken cancellationToken)
    {
        var permissions = User.FindAll(Permissions.ClaimType).Select(claim => claim.Value).ToHashSet();
        var username = User.FindFirst("unique_name")?.Value ?? "usuario";
        return Ok(await sender.Send(new AssistantChatCommand(request.Messages, username, permissions), cancellationToken));
    }
}
