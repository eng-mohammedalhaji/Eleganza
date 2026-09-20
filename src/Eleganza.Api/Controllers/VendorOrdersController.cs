using Eleganza.Application.Orders;
using Eleganza.Contracts.Orders;
using Eleganza.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eleganza.Api.Controllers;

[ApiController]
[Authorize(Roles = "VendorOwner,Admin")]
[Route("api/vendor/orders")]
public sealed class VendorOrdersController(OrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> List(
        [FromQuery] OrderStatus? status,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
        => Ok(await orderService.ListVendorAsync(status, take, cancellationToken));

    [HttpPost("{id:guid}/confirm")]
    public async Task<ActionResult<OrderResponse>> Confirm(Guid id, CancellationToken cancellationToken)
        => Ok(await orderService.ConfirmAsync(id, cancellationToken));

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<OrderResponse>> Reject(
        Guid id,
        [FromBody] string? reason,
        CancellationToken cancellationToken)
        => Ok(await orderService.RejectAsync(id, reason, cancellationToken));

    [HttpPost("{id:guid}/delivered")]
    public async Task<ActionResult<OrderResponse>> Delivered(Guid id, CancellationToken cancellationToken)
        => Ok(await orderService.MarkDeliveredAsync(id, cancellationToken));

    [HttpPost("{id:guid}/retry-shipping")]
    public async Task<ActionResult<OrderResponse>> RetryShipping(Guid id, CancellationToken cancellationToken)
        => Ok(await orderService.RetryShippingAsync(id, cancellationToken));

    [HttpPost("{id:guid}/collected")]
    public async Task<ActionResult<OrderResponse>> Collected(Guid id, CancellationToken cancellationToken)
        => Ok(await orderService.MarkCollectedAsync(id, cancellationToken));
}
