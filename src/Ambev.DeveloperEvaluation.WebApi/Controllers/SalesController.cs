using Ambev.DeveloperEvaluation.Application.Commands.CanselSale;
using Ambev.DeveloperEvaluation.Application.Commands.CreateSale;
using Ambev.DeveloperEvaluation.Application.Commands.UpdateSale;
using Ambev.DeveloperEvaluation.Application.queries.GetSale;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery(Name = "_page")] int page = 1,
        [FromQuery(Name = "_size")] int size = 10,
        [FromQuery(Name = "_order")] string? order = null,
        CancellationToken cancellationToken = default)
    {
        var reserved = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "_page", "_size", "_order" };
        var filters = HttpContext.Request.Query
            .Where(q => !reserved.Contains(q.Key))
            .ToDictionary(q => q.Key, q => q.Value.ToString());

        var query = new GetSalesQuery(page, size, order, filters.Count > 0 ? filters : null);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSaleQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new CancelSaleCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{saleId:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> CancelItem(Guid saleId, Guid itemId, CancellationToken cancellationToken)
    {
        await mediator.Send(new CancelSaleItemCommand(saleId, itemId), cancellationToken);
        return NoContent();
    }
}
