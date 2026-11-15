using Magazing.Application.DTOs.Sales;
using Magazing.Application.Security;
using Magazing.Application.UseCases.Customers.Commands;
using Magazing.Application.UseCases.Customers.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.SalesRead)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        return Ok(await sender.Send(new GetCustomersQuery(page, pageSize, search, isActive)));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.SalesRead)]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await sender.Send(new GetCustomerByIdQuery(id)));
    }

    [HttpPost]
    [Authorize(Policy = Permissions.CustomersManage)]
    public async Task<IActionResult> Create([FromBody] CustomerRequest request)
    {
        var customer = await sender.Send(new CreateCustomerCommand(
            request.DocumentType, request.DocumentNumber, request.FullName, request.Email, request.Phone, request.Address));

        return CreatedAtAction(nameof(GetById), new { id = customer.CustomerId }, customer);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.CustomersManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request)
    {
        return Ok(await sender.Send(new UpdateCustomerCommand(
            id, request.DocumentType, request.DocumentNumber, request.FullName, request.Email, request.Phone, request.Address, request.IsActive)));
    }
}
