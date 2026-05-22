namespace Aras.External;

public interface IArasTraderClient
{
    Task<IReadOnlyList<ArasTraderCustomerDto>> GetCustomersAsync(CancellationToken cancellationToken);
}
