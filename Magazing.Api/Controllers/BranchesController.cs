using Magazing.Application.DTOs.Settings;
using Magazing.Application.Services.Security;
using Magazing.Application.UseCases.Branches.Commands;
using Magazing.Application.UseCases.Branches.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BranchesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await sender.Send(new GetBranchesQuery()));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> Create([FromBody] BranchRequest request)
    {
        var branch = await sender.Send(new CreateBranchCommand(request.Code, request.Name, request.Address, request.Phone, request.IsActive));
        return StatusCode(StatusCodes.Status201Created, branch);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] BranchRequest request)
    {
        return Ok(await sender.Send(new UpdateBranchCommand(id, request.Code, request.Name, request.Address, request.Phone, request.IsActive)));
    }
}
