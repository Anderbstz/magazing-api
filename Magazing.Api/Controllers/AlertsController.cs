using Magazing.Api.Configuration.Authorization;
using Magazing.Application.Services.Security;
using Magazing.Application.UseCases.Alerts.Commands;
using Magazing.Application.UseCases.Alerts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.AlertsManage)]
public class AlertsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null)
    {
        return Ok(await sender.Send(new GetAlertsQuery(status)));
    }

    [HttpPost("{id:guid}/acknowledge")]
    public async Task<IActionResult> Acknowledge(Guid id)
    {
        return Ok(await sender.Send(new AcknowledgeAlertCommand(id)));
    }

    [HttpPost("{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid id)
    {
        return Ok(await sender.Send(new ResolveAlertCommand(id, User.GetUserId())));
    }
}
