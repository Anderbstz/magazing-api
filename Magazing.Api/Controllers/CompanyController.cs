using Magazing.Application.DTOs.Settings;
using Magazing.Application.Services.Security;
using Magazing.Application.UseCases.Company.Commands;
using Magazing.Application.UseCases.Company.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/company")]
[Authorize]
public class CompanyController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await sender.Send(new GetCompanyQuery()));
    }

    [HttpPut]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> Update([FromBody] CompanyRequest request)
    {
        return Ok(await sender.Send(new UpdateCompanyCommand(
            request.Ruc, request.BusinessName, request.TradeName, request.Address, request.Phone, request.Email, request.LogoUrl)));
    }
}
