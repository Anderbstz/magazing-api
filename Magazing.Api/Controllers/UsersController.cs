using Magazing.Api.Configuration.Authorization;
using Magazing.Application.DTOs.Users;
using Magazing.Application.Services.Security;
using Magazing.Application.UseCases.Users.Commands;
using Magazing.Application.UseCases.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.UsersRead)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        return Ok(await sender.Send(new GetUsersQuery(page, pageSize, search)));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.UsersRead)]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await sender.Send(new GetUserByIdQuery(id)));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand request)
    {
        var access = await sender.Send(request);
        return CreatedAtAction(nameof(GetById), new { id = access.User.UserId }, access);
    }

    [HttpPost("{id:guid}/reset-access")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> ResetAccess(Guid id)
    {
        return Ok(await sender.Send(new ResetUserAccessCommand(id)));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        return Ok(await sender.Send(new UpdateUserCommand(id, request.Email, request.FullName, request.BranchId, request.IsActive, User.GetUserId())));
    }

    [HttpPut("{id:guid}/roles")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> AssignRoles(Guid id, [FromBody] AssignRolesRequest request)
    {
        return Ok(await sender.Send(new AssignUserRolesCommand(id, request.RoleIds, User.GetUserId())));
    }
}
