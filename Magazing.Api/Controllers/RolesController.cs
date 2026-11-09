using Magazing.Application.DTOs.Roles;
using Magazing.Application.Security;
using Magazing.Application.UseCases.Roles.Commands;
using Magazing.Application.UseCases.Roles.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.UsersRead)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await sender.Send(new GetRolesQuery()));
    }

    [HttpGet("permissions")]
    [Authorize(Policy = Permissions.UsersRead)]
    public async Task<IActionResult> GetPermissions()
    {
        return Ok(await sender.Send(new GetPermissionsQuery()));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> Create([FromBody] CreateRoleCommand request)
    {
        var role = await sender.Send(request);
        return CreatedAtAction(nameof(GetAll), null, role);
    }

    [HttpPut("{id:guid}/permissions")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> UpdatePermissions(Guid id, [FromBody] UpdateRolePermissionsRequest request)
    {
        return Ok(await sender.Send(new UpdateRolePermissionsCommand(id, request.PermissionIds)));
    }
}
