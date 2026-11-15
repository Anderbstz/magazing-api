using Magazing.Application.Security;
using Magazing.Application.UseCases.PaymentMethods.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/payment-methods")]
[Authorize(Policy = Permissions.SalesRead)]
public class PaymentMethodsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await sender.Send(new GetPaymentMethodsQuery()));
    }
}
