using Aras.Contracts;
using Aras.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aras.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(IOrderGateway gateway, IOrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderResponse>>> Get(CancellationToken cancellationToken)
    {
        return Ok(await orderService.GetOrdersAsync(cancellationToken));
    }

    [HttpPost("addorder")]
    public async Task<ActionResult<OrderResponse>> AddOrder(OrderCreateRequest request, CancellationToken cancellationToken)
    {
        var order = await gateway.AddOrderAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    [HttpPut("editorder/{id:guid}")]
    public async Task<ActionResult<OrderResponse>> EditOrder(Guid id, OrderEditRequest request, CancellationToken cancellationToken)
    {
        return Ok(await gateway.EditOrderAsync(id, request, cancellationToken));
    }
}
