using Eleganza.Application.Orders;
using Eleganza.Contracts.Orders;
using Eleganza.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eleganza.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/orders")]
public sealed class AdminOrdersController(OrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> List(
        [FromQuery] OrderStatus? status,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
        => Ok(await orderService.ListAdminAsync(status, take, cancellationToken));
}
