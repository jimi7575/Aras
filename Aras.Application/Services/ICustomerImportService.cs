namespace Aras.Services;

public interface ICustomerImportService
{
    Task<int> ImportAsync(CancellationToken cancellationToken);
}
