using Eleganza.Application.Orders;
using Eleganza.Contracts.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eleganza.Api.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(OrderService orderService) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<OrderResponse>> Create(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
        => StatusCode(StatusCodes.Status201Created, await orderService.CreateAsync(request, cancellationToken));

    [HttpGet("mine")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> Mine(CancellationToken cancellationToken)
        => Ok(await orderService.ListMineAsync(cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    [Authorize]
    public async Task<ActionResult<OrderResponse>> Cancel(
        Guid id,
        [FromBody] string? reason,
        CancellationToken cancellationToken)
        => Ok(await orderService.CancelAsync(id, reason, cancellationToken));
}
