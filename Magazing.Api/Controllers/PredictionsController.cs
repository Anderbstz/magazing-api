using Magazing.Application.Security;
using Magazing.Application.UseCases.Predictions.Commands;
using Magazing.Application.UseCases.Predictions.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Magazing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PredictionsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.InventoryRead)]
    public async Task<IActionResult> GetForecast([FromQuery] Guid productId, [FromQuery] Guid? warehouseId = null)
    {
        return Ok(await sender.Send(new GetDemandForecastQuery(productId, warehouseId)));
    }

    [HttpPost("run")]
    [Authorize(Policy = Permissions.PredictionsRun)]
    public async Task<IActionResult> Run()
    {
        return Ok(await sender.Send(new RunPredictionsCommand()));
    }
}
