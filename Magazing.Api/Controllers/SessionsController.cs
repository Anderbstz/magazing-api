using Magazing.Api.Configuration.Authorization;
using Magazing.Application.Services.Security;
using Magazing.Application.UseCases.Users.Commands;
using Magazing.Application.UseCases.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.UsersManage)]
public class SessionsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetActive([FromQuery] Guid? userId)
    {
        return Ok(await sender.Send(new GetActiveSessionsQuery(userId)));
    }

    [HttpPost("revoke-inactive")]
    public async Task<IActionResult> RevokeInactive([FromQuery] int hours = 24)
    {
        var revoked = await sender.Send(new RevokeInactiveSessionsCommand(hours, User.GetSessionId()));
        return Ok(new { revoked });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Revoke(Guid id)
    {
        await sender.Send(new RevokeSessionCommand(id));
        return NoContent();
    }
}
