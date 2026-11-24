using Magazing.Application.DTOs.Settings;
using Magazing.Application.Services.Security;
using Magazing.Application.UseCases.Series.Commands;
using Magazing.Application.UseCases.Series.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/document-series")]
[Authorize]
public class DocumentSeriesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await sender.Send(new GetDocumentSeriesQuery()));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> Create([FromBody] DocumentSeriesRequest request)
    {
        var series = await sender.Send(new CreateDocumentSeriesCommand(request.BranchId, request.DocumentType, request.Series));
        return StatusCode(StatusCodes.Status201Created, series);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> SetActive(Guid id, [FromBody] SetActiveRequest request)
    {
        return Ok(await sender.Send(new SetDocumentSeriesActiveCommand(id, request.IsActive)));
    }
}
