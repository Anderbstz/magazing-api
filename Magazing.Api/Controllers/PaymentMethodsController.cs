using Magazing.Application.DTOs.Settings;
using Magazing.Application.Security;
using Magazing.Application.UseCases.PaymentMethods.Commands;
using Magazing.Application.UseCases.PaymentMethods.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/payment-methods")]
[Authorize]
public class PaymentMethodsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.SalesRead)]
    public async Task<IActionResult> GetAll([FromQuery] bool all = false)
    {
        return Ok(await sender.Send(new GetPaymentMethodsQuery(all)));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentMethodRequest request)
    {
        return Ok(await sender.Send(new UpdatePaymentMethodCommand(id, request.Name, request.IsActive)));
    }
}
