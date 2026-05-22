using Aras.Contracts;
using Aras.Domain;

namespace Aras.Services;

public static class OrderMappings
{
    public static OrderResponse ToResponse(this Order order)
    {
        return new OrderResponse(
            order.Id,
            order.CustomerId,
            order.Side,
            order.Amount,
            order.Description,
            order.Status,
            order.CreatedAtUtc,
            order.AppliedAtUtc);
    }
}
