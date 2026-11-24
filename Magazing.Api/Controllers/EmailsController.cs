using Magazing.Application.Services.Security;
using Magazing.Application.UseCases.Emails.Commands;
using Magazing.Application.UseCases.Emails.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.UsersManage)]
public class EmailsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? template = null)
    {
        return Ok(await sender.Send(new GetEmailsQuery(status, search, page, pageSize, template)));
    }

    [HttpPost("{id:guid}/retry")]
    public async Task<IActionResult> Retry(Guid id)
    {
        return Ok(await sender.Send(new RetryEmailCommand(id)));
    }
}
