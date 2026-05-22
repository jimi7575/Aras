using Aras.Domain;

namespace Aras.Contracts;

public sealed record OrderResponse(
    Guid Id,
    int CustomerId,
    OrderSide Side,
    decimal Amount,
    string? Description,
    OrderStatus Status,
    DateTime CreatedAtUtc,
    DateTime? AppliedAtUtc);
