namespace Aras.Contracts;

public sealed record WalletResponse(int CustomerId, decimal Balance, DateTime UpdatedAtUtc);
